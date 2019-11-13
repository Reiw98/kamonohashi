using Microsoft.EntityFrameworkCore;
using Nssol.Platypus.DataAccess.Core;
using Nssol.Platypus.DataAccess.Repositories.Interfaces;
using Nssol.Platypus.Models;
using System.Collections.Generic;
using System.Linq;

namespace Nssol.Platypus.DataAccess.Repositories
{
    /// <summary>
    /// クラスタテーブルにアクセスするためのリポジトリクラス
    /// </summary>
    /// <seealso cref="Nssol.Platypus.DataAccess.Repositories.Interfaces.IClusterRepository" />
    public class ClusterRepository : RepositoryBase<Cluster>, IClusterRepository
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context">データにアクセスするためのDBコンテキスト</param>
        public ClusterRepository(CommonDbContext context) : base(context)
        {
        }

        /// <summary>
        /// 指定したクラスタにアクセス可能なテナント一覧を返す。
        /// クラスタIDの存在チェックは行わない。
        /// </summary>
        /// <param name="clusterId">対象クラスタ</param>
        public IEnumerable<Tenant> GetAssignedTenants(long clusterId)
        {
            return FindModelAll<ClusterTenantMap>(map => map.ClusterId == clusterId).Include(map => map.Tenant).Select(map => map.Tenant);
        }

        /// <summary>
        /// 指定したクラスタに対するテナントのアサイン状況をリセットする。
        /// </summary>
        /// <param name="clusterId">対象クラスタ</param>
        public void ResetAssinedTenants(long clusterId)
        {
            DeleteModelAll<ClusterTenantMap>(map => map.ClusterId == clusterId);
        }

        /// <summary>
        /// 指定したクラスタにテナントをアサインする。
        /// テナントIDの存在チェックは行わない。
        /// </summary>
        /// <param name="cluster">対象クラスタ</param>
        /// <param name="tenantIds">テナントID一覧</param>
        /// <param name="isCreate">新規登録時であればtrue</param>
        public void AssignTenants(Cluster cluster, IEnumerable<long> tenantIds, bool isCreate)
        {
            foreach (long tenantId in tenantIds)
            {
                var map = new ClusterTenantMap()
                {
                    TenantId = tenantId
                };
                if (isCreate)
                {
                    map.Cluster = cluster;
                }
                else
                {
                    map.ClusterId = cluster.Id;
                }
                AddModel<ClusterTenantMap>(map);
            }
        }

        /// <summary>
        /// 指定したテナントがアクセス可能なクラスタ一覧を取得する。
        /// テナントIDの存在チェックは行わない。
        /// </summary>
        /// <param name="tenantId">対象テナントID</param>
        public IEnumerable<Cluster> GetAccessibleClusters(long tenantId)
        {
            //テナントがアクセス可能なクラスタID一覧を取得
            var clusterIds = FindModelAll<ClusterTenantMap>(map => map.TenantId == tenantId).Select(map => map.ClusterId).ToList();

            //アクセス可能なクラスタ一覧を取得
            return GetAll().Where(c => clusterIds.Contains(c.Id));
        }
    }
}
