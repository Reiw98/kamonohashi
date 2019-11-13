using Nssol.Platypus.Models;
using System.Collections.Generic;

namespace Nssol.Platypus.ApiModels.ClusterApiModels
{
    /// <summary>
    /// クラスタ詳細情報
    /// </summary>
    public class DetailsOutputModel : IndexOutputModel
    {
        public DetailsOutputModel(Cluster cluster) : base(cluster)
        {
            ResourceManageKey = cluster.ResourceManageKey;
        }

        /// <summary>
        /// リソース管理キー。
        /// クラスタのトークン。
        /// </summary>
        public string ResourceManageKey { get; set; }

        /// <summary>
        /// このクラスタを使用できるテナントの一覧。
        /// </summary>
        public IEnumerable<AssignedTenant> AssignedTenants { get; set; }

        /// <summary>
        /// テナント情報
        /// </summary>
        public class AssignedTenant
        {
            /// <summary>
            /// テナントID
            /// </summary>
            public long Id { get; set; }
            /// <summary>
            /// テナント名
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// テナント表示名
            /// </summary>
            public string DisplayName { get; set; }
        }
    }
}
