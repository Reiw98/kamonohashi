using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nssol.Platypus.ApiModels.ClusterApiModels;
using Nssol.Platypus.Controllers.Util;
using Nssol.Platypus.DataAccess.Core;
using Nssol.Platypus.DataAccess.Repositories.Interfaces;
using Nssol.Platypus.DataAccess.Repositories.Interfaces.TenantRepositories;
using Nssol.Platypus.Filters;
using Nssol.Platypus.Infrastructure;
using Nssol.Platypus.Infrastructure.Infos;
using Nssol.Platypus.Infrastructure.Types;
using Nssol.Platypus.Logic.Interfaces;
using Nssol.Platypus.Models;
using Nssol.Platypus.Models.TenantModels;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Nssol.Platypus.Controllers.spa
{
    [Route("api/v1")]
    public class ClusterController : PlatypusApiControllerBase
    {
        private readonly IClusterRepository clusterRepository;
        private readonly ITensorBoardContainerRepository tensorBoardContainerRepository;
        private readonly IClusterManagementLogic clusterManagementLogic;
        private readonly IUnitOfWork unitOfWork;

        public ClusterController(
            IClusterRepository clusterRepository,
            ITensorBoardContainerRepository tensorBoardContainerRepository,
            IClusterManagementLogic clusterManagementLogic,
            IUnitOfWork unitOfWork,
            IHttpContextAccessor accessor) : base(accessor)
        {
            this.clusterRepository = clusterRepository;
            this.tensorBoardContainerRepository = tensorBoardContainerRepository;
            this.clusterManagementLogic = clusterManagementLogic;
            this.unitOfWork = unitOfWork;
        }

        #region クラスタ管理

        /// <summary>
        /// 全クラスタ一覧を取得する
        /// </summary>
        [HttpGet("/api/v1/admin/cluster")]
        [PermissionFilter(MenuCode.Cluster)]
        [ProducesResponseType(typeof(IEnumerable<IndexOutputModel>), (int)HttpStatusCode.OK)]
        public IActionResult GetAll()
        {
            var clusters = clusterRepository.GetAll();

            return JsonOK(clusters.Select(c => new IndexOutputModel(c)));
        }

        /// <summary>
        /// 指定したIDのクラスタ情報を取得する
        /// </summary>
        /// <param name="id">取得対象のクラスタID</param>
        [HttpGet("/api/v1/admin/cluster/{id}")]
        [PermissionFilter(MenuCode.Cluster)]
        [ProducesResponseType(typeof(DetailsOutputModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetDetail(long? id)
        {
            // データの入力チェック
            if (!id.HasValue)
            {
                return JsonBadRequest("Cluster ID is required.");
            }
            // データの存在チェック
            var cluster = await clusterRepository.GetByIdAsync(id.Value);
            if (cluster == null)
            {
                return JsonNotFound($"Cluster Id {id.Value} is not found.");
            }

            var model = new DetailsOutputModel(cluster)
            {
                // アクセス可能なテナント情報を取得する
                AssignedTenants = clusterRepository.GetAssignedTenants(cluster.Id).Select(t => new DetailsOutputModel.AssignedTenant()
                {
                    Id = t.Id,
                    Name = t.Name,
                    DisplayName = t.DisplayName
                })
            };

            return JsonOK(model);
        }

        /// <summary>
        /// 新規にクラスタを登録する
        /// </summary>
        /// <param name="model">登録するクラスタ情報</param>
        /// <param name="tenantRepository">DI用</param>
        [HttpPost("/api/v1/admin/cluster")]
        [PermissionFilter(MenuCode.Cluster)]
        [ProducesResponseType(typeof(IndexOutputModel), (int)HttpStatusCode.Created)]
        public async Task<IActionResult> Create([FromBody]CreateInputModel model,
            [FromServices] ITenantRepository tenantRepository)
        {
            // データの入力チェック
            if (!ModelState.IsValid)
            {
                return JsonBadRequest("Invalid inputs.");
            }

            var cluster = new Cluster()
            {
                DisplayName = model.DisplayName,
                HostName = model.HostName,
                PortNo = model.PortNo,
                ResourceManageKey = model.ResourceManageKey,
                Memo = model.Memo
            };

            // テナントをアサイン
            if (model.AssignedTenantIds != null)
            {
                foreach (long tenantId in model.AssignedTenantIds)
                {
                    var tenant = tenantRepository.Get(tenantId);
                    if (tenant == null)
                    {
                        return JsonNotFound($"Tenant ID {tenantId} is not found.");
                    }
                    // コンテナ管理サービスにテナントを登録する
                    var clusterResult = await clusterManagementLogic.RegistTenantAsync(tenant.Name, cluster);
                    if (clusterResult == false)
                    {
                        return JsonError(HttpStatusCode.ServiceUnavailable, "Couldn't create cluster master namespace. Please check the configuration to the connect cluster manager service.");
                    }
                }
                clusterRepository.AssignTenants(cluster, model.AssignedTenantIds, true);
            }

            clusterRepository.Add(cluster);
            unitOfWork.Commit();

            return JsonCreated(new IndexOutputModel(cluster));
        }

        /// <summary>
        /// 指定したIDのクラスタ情報を編集する
        /// </summary>
        /// <param name="id">編集対象のクラスタID</param>
        /// <param name="model">編集するクラスタ情報</param>
        /// <param name="tenantRepository">DI用</param>
        [HttpPut("/api/v1/admin/cluster/{id}")]
        [PermissionFilter(MenuCode.Cluster)]
        [ProducesResponseType(typeof(IndexOutputModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Edit(long? id, [FromBody]CreateInputModel model,
            [FromServices] ITenantRepository tenantRepository) // EditとCreateで項目が同じなので、入力モデルを使いまわし
        {
            // データの入力チェック
            if (!ModelState.IsValid || !id.HasValue)
            {
                return JsonBadRequest("Invalid inputs.");
            }
            // データの存在チェック
            var cluster = await clusterRepository.GetByIdAsync(id.Value);
            if (cluster == null)
            {
                return JsonNotFound($"Cluster ID {id.Value} is not found.");
            }

            // アクセス可能なテナント一覧を取得する
            var oldTenants = clusterRepository.GetAssignedTenants(cluster.Id);
            foreach (Tenant tenant in oldTenants)
            {
                // コンテナ管理サービスのテナント情報を削除する
                await clusterManagementLogic.EraseTenantAsync(tenant.Name, cluster);
            }

            // コンテナ管理サービスへのテナント登録に失敗した際のために前のクラスタ情報を保持しておく
            var oldCluster = new Cluster()
            {
                HostName = cluster.HostName,
                PortNo = cluster.PortNo,
                ResourceManageKey = cluster.ResourceManageKey
            };

            // ClusterはCLIではなく画面から変更されるので、常にすべての値を入れ替える
            cluster.DisplayName = model.DisplayName;
            cluster.HostName = model.HostName;
            cluster.PortNo = model.PortNo;
            cluster.ResourceManageKey = model.ResourceManageKey;
            cluster.Memo = model.Memo;

            // まずは全てのアサイン情報を削除する
            clusterRepository.ResetAssinedTenants(cluster.Id);

            // テナントをアサイン
            if (model.AssignedTenantIds != null)
            {
                foreach (long tenantId in model.AssignedTenantIds)
                {
                    var tenant = tenantRepository.Get(tenantId);
                    if (tenant == null)
                    {
                        return JsonNotFound($"Tenant ID {tenantId} is not found.");
                    }
                    // コンテナ管理サービスにテナントを登録する
                    var clusterResult = await clusterManagementLogic.RegistTenantAsync(tenant.Name, cluster);
                    if (clusterResult == false)
                    {
                        // テナント情報の登録を元に戻す
                        foreach (Tenant oldTenant in oldTenants)
                        {
                            // コンテナ管理サービスにテナントを登録する（ここでの登録は前のクラスタ情報を使用するので失敗しないはず）
                            await clusterManagementLogic.RegistTenantAsync(oldTenant.Name, oldCluster);
                        }
                        return JsonError(HttpStatusCode.ServiceUnavailable, "Couldn't create cluster master namespace. Please check the configuration to the connect cluster manager service.");
                    }
                }
                clusterRepository.AssignTenants(cluster, model.AssignedTenantIds, false);
            }

            unitOfWork.Commit();

            return JsonOK(new IndexOutputModel(cluster));
        }

        /// <summary>
        /// 指定したIDのクラスタ情報を削除する
        /// </summary>
        /// <param name="id">削除対象のクラスタID</param>
        [HttpDelete("/api/v1/admin/cluster/{id}")]
        [PermissionFilter(MenuCode.Cluster)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> Delete(long? id)
        {
            //データの入力チェック
            if (!id.HasValue)
            {
                return JsonBadRequest("Invalid inputs.");
            }
            //データの存在チェック
            var cluster = await clusterRepository.GetByIdAsync(id.Value);
            if (cluster == null)
            {
                return JsonNotFound($"Node ID {id.Value} is not found.");
            }

            // アクセス可能なテナント一覧を取得する
            var tenants = clusterRepository.GetAssignedTenants(cluster.Id);

            string containerExistMessage = "";
            // コンテナ存在チェック
            foreach (Tenant tenant in tenants)
            {
                // 削除対象のクラスタ上で、コンテナ稼働中の場合は削除しない
                var containers = await clusterManagementLogic.GetAllContainerDetailsInfosAsync(tenant.Name, cluster);
                if (!containers.IsSuccess)
                {
                    JsonError(HttpStatusCode.ServiceUnavailable, $"ClusterManagementLogic#GetAllContainerDetailsInfosAsync() retusns error. tenantName=[{tenant.Name}]");
                }
                else if (containers.Value.Count() > 0)
                {
                    // ステータスによらず、全て稼働中と見做し、エラーメッセージ作成
                    if (!string.IsNullOrEmpty(containerExistMessage))
                    {
                        containerExistMessage += ", ";
                    }
                    containerExistMessage += $"tenant name=[{tenant.Name}], running container count=[{containers.Value.Count()}]";
                }
            }
            // コンテナ存在チェックの結果を確認
            if (!string.IsNullOrEmpty(containerExistMessage))
            {
                return JsonConflict($"Running containers exists deleting tenant. {containerExistMessage}");
            }

            foreach (Tenant tenant in tenants)
            {
                // コンテナ管理サービスのテナント情報を削除する
                await clusterManagementLogic.EraseTenantAsync(tenant.Name, cluster);
            }

            // 全てのアサイン情報を削除する
            clusterRepository.ResetAssinedTenants(cluster.Id);

            // クラスタ情報を削除する
            clusterRepository.Delete(cluster);

            unitOfWork.Commit();

            return JsonNoContent();
        }

        #endregion

        #region クラスタアクセス

        /// <summary>
        /// 接続中のテナントに有効なクラスタの一覧を取得する。
        /// </summary>
        [HttpGet("/api/v1/cluster")]
        [PermissionFilter(MenuCode.Training, MenuCode.Inference, MenuCode.Notebook)]
        [ProducesResponseType(typeof(IEnumerable<SimpleOutputModel>), (int)HttpStatusCode.OK)]
        public IActionResult GetClusters()
        {
            var clusters = clusterRepository.GetAccessibleClusters(CurrentUserInfo.SelectedTenant.Id);

            return JsonOK(clusters.Select(c => new SimpleOutputModel(c)));
        }

        #endregion

        #region パーティション

        /// <summary>
        /// 接続中のテナントに有効なパーティションの一覧を取得する。
        /// </summary>
        /// <param name="nodeRepository">DI用</param>
        [HttpGet("tenant/partitions")]
        [PermissionFilter(MenuCode.Training, MenuCode.Preprocess, MenuCode.Inference, MenuCode.Notebook)]
        [ProducesResponseType(typeof(IEnumerable<string>), (int)HttpStatusCode.OK)]
        public IActionResult GetPartitions([FromServices] INodeRepository nodeRepository)
        {
            //パーティション情報はDBから取得する。クラスタ側との同期は行わないので、必要なら別のAPIを呼ぶ仕様。
            var nodes = nodeRepository.GetAccessibleNodes(CurrentUserInfo.SelectedTenant.Id);
            var partitions = nodes.Select(n => n.Partition).Distinct().Where(p => string.IsNullOrEmpty(p) == false).OrderBy(e => e); //並び替えして返却
            return JsonOK(partitions);
        }

        /// <summary>
        /// パーティションの一覧を取得する。
        /// </summary>
        /// <param name="nodeRepository">DI用</param>
        [HttpGet("admin/partitions")]
        [PermissionFilter(MenuCode.Node)]
        [ProducesResponseType(typeof(IEnumerable<string>), (int)HttpStatusCode.OK)]
        public IActionResult GetPartitionsForAdmin([FromServices] INodeRepository nodeRepository)
        {
            //パーティション情報はDBから取得する。クラスタ側との同期は行わないので、必要なら別のAPIを呼ぶ仕様。
            var nodes = nodeRepository.GetAll();
            var partitions = nodes.Select(n => n.Partition).Distinct().Where(p => string.IsNullOrEmpty(p) == false).OrderBy(e => e); //並び替えして返却
            return JsonOK(partitions);
        }

        #endregion

        #region クォータ管理

        /// <summary>
        /// クォータ設定を取得する。
        /// </summary>
        /// <param name="tenantRepository">DI用</param>
        [HttpGet("admin/quotas")]
        [PermissionFilter(MenuCode.Quota)]
        [ProducesResponseType(typeof(IEnumerable<QuotaOutputModel>), (int)HttpStatusCode.OK)]
        public IActionResult GetQuotas([FromServices] ITenantRepository tenantRepository)
        {
            var result = tenantRepository.GetAllTenants().Select(t => new QuotaOutputModel(t));
            return JsonOK(result);
        }

        /// <summary>
        /// クォータ設定を更新する。
        /// </summary>
        /// <remarks>
        /// 0が指定された場合、上限なしを示す。また、指定のなかったテナントは更新しない。
        /// </remarks>
        /// <param name="models">クォータ更新用モデル</param>
        /// <param name="tenantRepository">DI用</param>
        [HttpPost("admin/quotas")]
        [PermissionFilter(MenuCode.Quota)]
        [ProducesResponseType(typeof(IEnumerable<QuotaOutputModel>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> EditQuotas([FromBody] IEnumerable<QuotaInputModel> models, [FromServices] ITenantRepository tenantRepository)
        {
            //データの入力チェック
            if (!ModelState.IsValid)
            {
                return JsonBadRequest("Invalid inputs.");
            }

            var result = new List<QuotaOutputModel>();

            foreach (var inputModel in models)
            {
                //更新用に、キャッシュではなくDBから直接取得
                Tenant tenant = await tenantRepository.GetTenantForUpdateAsync(inputModel.TenantId.Value);
                if (tenant == null)
                {
                    return JsonNotFound($"Tenant ID {inputModel.TenantId.Value} is not found.");
                }
                tenant.LimitCpu = inputModel.Cpu == 0 ? (int?)null : inputModel.Cpu;
                tenant.LimitMemory = inputModel.Memory == 0 ? (int?)null : inputModel.Memory;
                tenant.LimitGpu = inputModel.Gpu == 0 ? (int?)null : inputModel.Gpu;

                //結果に格納
                result.Add(new QuotaOutputModel(tenant));
                
                await clusterManagementLogic.SetQuotaAsync(tenant);
            }

            unitOfWork.Commit();
            tenantRepository.Refresh();

            return JsonOK(result);
        }

        #endregion

        #region TensorBoard

        /// <summary>
        /// DB上の全てのTensorBoardコンテナ情報を対応する実コンテナごと削除する。
        /// </summary>
        /// <remarks>
        /// REST APIとして定時バッチから実行される想定。
        /// </remarks>
        [HttpDelete("admin/tensorboards")]
        [PermissionFilter(MenuCode.Node)]
        [ProducesResponseType(typeof(int), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> DeleteAll()
        {
            var containers = await tensorBoardContainerRepository.GetAllIncludePortAndTenantAsync();

            int count = 0; //削除したコンテナの数
            bool failure = false; //1件でも失敗したか

            LogInformation("コンテナのDBレコード削除、コンテナ強制終了を開始。");

            foreach (TensorBoardContainer container in containers)
            {
                var destroyResult = await clusterManagementLogic.DeleteContainerAsync(ContainerType.TensorBoard, container.Name, container.Tenant.Name, null, true);

                //コンテナ削除に成功した場合
                if (destroyResult)
                {
                    count++;
                }
                else
                {
                    LogError($"テナント:{container.Tenant.Name}のコンテナ:{container.Name}のコンテナ強制終了時に予期しないエラーが発生しました。");

                    //TensorBoard削除に限り、ミスって削除しても問題が大きくないため、途中でエラーが発生しても最後まで処理し続ける

                    failure = true;
                }

                //DB側も削除する
                //コンテナの削除に失敗したとしても、ログは出しているし、DBに残していてもできる事がないので、無視して削除する
                tensorBoardContainerRepository.Delete(container, true);
            }
            unitOfWork.Commit();
            if(failure)
            {
                //1件以上失敗しているので、エラー扱い
                return JsonError(HttpStatusCode.ServiceUnavailable, $"failed to delete some tensorboard containers. deleted: {count}");
            }
            else
            {
                return JsonOK(count);
            }
        }

        #endregion

        /// <summary>
        /// イベントを取得する
        /// </summary>
        /// <param name="id">テナントID</param>
        /// <param name="name">コンテナ名</param>
        /// <param name="tenantRepository">DI用</param>
        [HttpGet("admin/events/{id}")]
        [PermissionFilter(MenuCode.Tenant)]
        [ProducesResponseType(typeof(IEnumerable<ContainerEventInfo>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetEvents([FromRoute] long? id, [FromQuery] string name, [FromServices] ITenantRepository tenantRepository)
        {
            if (id == null)
            {
                return JsonBadRequest("Tenant ID is required.");
            }

            Tenant tenant = tenantRepository.Get(id.Value);
            if (tenant == null)
            {
                return JsonNotFound($"Tenant Id {id.Value} is not found.");
            }

            var result = string.IsNullOrEmpty(name) ?
                await clusterManagementLogic.GetEventsAsync(tenant, null, true) :
                await clusterManagementLogic.GetEventsAsync(tenant, name, null, true, false);
            if (result.IsSuccess)
            {
                return JsonOK(result.Value);
            }
            else
            {
                return JsonError(HttpStatusCode.ServiceUnavailable, $"failed to access to container: {result.Error.Name}");
            }
        }
    }
}