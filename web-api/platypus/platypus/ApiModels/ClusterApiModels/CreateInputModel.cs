using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Nssol.Platypus.ApiModels.ClusterApiModels
{
    /// <summary>
    /// 新規登録に必要なクラスタ情報を保持する
    /// </summary>
    public class CreateInputModel
    {
        /// <summary>
        /// ホスト名
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

        /// <summary>
        /// このクラスタを使用できるテナントのID。
        /// </summary>
        public IEnumerable<long> AssignedTenantIds { get; set; }
    }
}
