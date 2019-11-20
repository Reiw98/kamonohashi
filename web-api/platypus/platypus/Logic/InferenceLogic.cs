using Nssol.Platypus.DataAccess.Core;
using Nssol.Platypus.DataAccess.Repositories.Interfaces;
using Nssol.Platypus.DataAccess.Repositories.Interfaces.TenantRepositories;
using Nssol.Platypus.Infrastructure;
using Nssol.Platypus.Infrastructure.Types;
using Nssol.Platypus.Logic.Interfaces;
using Nssol.Platypus.Models;
using Nssol.Platypus.Models.TenantModels;
using System;
using System.Threading.Tasks;

namespace Nssol.Platypus.Logic
{
    /// <summary>
    /// 推論ロジッククラス
    /// </summary>
    /// <seealso cref="Nssol.Platypus.Logic.Interfaces.IInferenceLogic" />
    public class InferenceLogic : PlatypusLogicBase, IInferenceLogic
    {
        private readonly IInferenceHistoryRepository inferenceHistoryRepository;
        private readonly IClusterRepository clusterRepository;
        private readonly IClusterManagementLogic clusterManagementLogic;
        private readonly IUnitOfWork unitOfWork;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public InferenceLogic(
            IInferenceHistoryRepository inferenceHistoryRepository,
            IClusterRepository clusterRepository,
            IClusterManagementLogic clusterManagementLogic,
            IUnitOfWork unitOfWork,
            ICommonDiLogic commonDiLogic) : base(commonDiLogic)
        {
            this.inferenceHistoryRepository = inferenceHistoryRepository;
            this.clusterRepository = clusterRepository;
            this.clusterManagementLogic = clusterManagementLogic;
            this.unitOfWork = unitOfWork;
        }

        /// <summary>
        /// 推論履歴コンテナを削除し、ステータスを変更する。
        /// </summary>
        /// <param name="inferenceHistory">対象学習履歴</param>
        /// <param name="status">変更後のステータス</param>
        /// <param name="force">他テナントに対する変更を許可するか</param>
        public async Task ExitAsync(InferenceHistory inferenceHistory, ContainerStatus status, bool force)
        {
            // コンテナの生存確認
            if (inferenceHistory.GetStatus().Exist())
            {
                // クラスタが設定されている場合、クラスタ情報を取得する
                Cluster cluster = null;
                if (inferenceHistory.ClusterId.HasValue)
                {
                    cluster = await clusterRepository.GetByIdAsync(inferenceHistory.ClusterId.Value);
                }

                var info = await clusterManagementLogic.GetContainerDetailsInfoAsync(inferenceHistory.Key, CurrentUserInfo.SelectedTenant.Name, cluster, force);

                // コンテナ削除の前に、DBの更新を先に実行
                await inferenceHistoryRepository.UpdateStatusAsync(inferenceHistory.Id, status, info.CreatedAt, DateTime.Now, force);

                // 実コンテナ削除の結果は確認せず、DBの更新を先に確定する（コンテナがいないなら、そのまま消しても問題ない想定）
                unitOfWork.Commit();

                if (info.Status.Exist())
                {
                    // 再確認してもまだ存在していたら、コンテナ削除
                    await clusterManagementLogic.DeleteContainerAsync(
                        ContainerType.Training, inferenceHistory.Key, CurrentUserInfo.SelectedTenant.Name, cluster, force);
                }
            }
            else
            {
                await inferenceHistoryRepository.UpdateStatusAsync(inferenceHistory.Id, status, force);

                // DBの更新を確定する
                unitOfWork.Commit();
            }
        }
    }
}
