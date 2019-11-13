using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nssol.Platypus.Models
{
    /// <summary>
    /// クラスタとテナントの対応付け中間テーブル。
    /// <seealso cref="Cluster"/>
    /// </summary>
    public class ClusterTenantMap : ModelBase
    {
        /// <summary>
        /// クラスタID
        /// </summary>
        [Required]
        public long ClusterId { get; set; }

        /// <summary>
        /// テナントID
        /// </summary>
        [Required]
        public long TenantId { get; set; }

        /// <summary>
        /// クラスタ
        /// </summary>
        [ForeignKey(nameof(ClusterId))]
        public virtual Cluster Cluster { get; set; }

        /// <summary>
        /// テナント
        /// </summary>
        [ForeignKey(nameof(TenantId))]
        public virtual Tenant Tenant { get; set; }
    }
}
