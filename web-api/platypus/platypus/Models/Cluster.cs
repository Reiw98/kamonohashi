using System.ComponentModel.DataAnnotations;

namespace Nssol.Platypus.Models
{
    /// <summary>
    /// クラスタ
    /// </summary>
    public class Cluster : ModelBase
    {
        /// <summary>
        /// クラスタホスト名
        /// </summary>
        [Required]
        public string HostName { get; set; }

        /// <summary>
        /// 表示名
        /// </summary>
        [Required]
        public string DisplayName { get; set; }

        /// <summary>
        /// リソース管理キー。
        /// クラスタのトークン。
        /// </summary>
        [Required]
        public string ResourceManageKey { get; set; }

        /// <summary>
        /// メモ
        /// </summary>
        public string Memo { get; set; }
    }
}
