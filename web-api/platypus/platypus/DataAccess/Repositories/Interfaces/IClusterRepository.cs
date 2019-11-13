using Nssol.Platypus.DataAccess.Core;
using Nssol.Platypus.Models;
using System.Collections.Generic;

namespace Nssol.Platypus.DataAccess.Repositories.Interfaces
{
    /// <summary>
    /// クラスタにアクセスするためのリポジトリインターフェース
    /// </summary>
    public interface IClusterRepository : IRepository<Cluster>
    {
        /// <summary>
        /// 指定したクラスタにアクセス可能なテナント一覧を返す。
        /// クラスタIDの存在チェックは行わない。
        /// </summary>
        /// <param name="clusterId">対象クラスタ</param>
        IEnumerable<Tenant> GetAssignedTenants(long clusterId);

        /// <summary>
        /// 指定したクラスタに対するテナントのアサイン状況をリセットする。
        /// </summary>
        /// <param name="clusterId">対象クラスタ</param>
        void ResetAssinedTenants(long clusterId);

        /// <summary>
        /// 指定したクラスタにテナントをアサインする。
        /// テナントIDの存在チェックは行わない。
        /// </summary>
        /// <param name="cluster">対象クラスタ</param>
        /// <param name="tenantIds">対象テナントID一覧</param>
        /// <param name="isCreate">新規登録時であればtrue</param>
        void AssignTenants(Cluster cluster, IEnumerable<long> tenantIds, bool isCreate);

        /// <summary>
        /// 指定したテナントがアクセス可能なクラスタ一覧を取得する。
        /// テナントIDの存在チェックは行わない。
        /// </summary>
        /// <param name="tenantId">対象テナントID</param>
        IEnumerable<Cluster> GetAccessibleClusters(long tenantId);
    }
}
