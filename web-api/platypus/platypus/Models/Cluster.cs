using System.ComponentModel.DataAnnotations;

namespace Nssol.Platypus.Models
{
    /// <summary>
    /// クラスタ
    /// </summary>
    public class Cluster : ModelBase
    {
        /// <summary>
        /// 表示名
        /// </summary>
        [Required]
        public string DisplayName { get; set; }

        /// <summary>
        /// クラスタホスト名
        /// </summary>
        [Required]
        public string HostName { get; set; }

        /// <summary>
        /// ポート番号
        /// </summary>
        public int PortNo { get; set; }
        
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
        /// クラスタサービス(e.g. k8s)のベースURL
        /// </summary>
        public string ServiceBaseUrl
        {
            get
            {
                return $"https://{ HostName }:{ PortNo }";
            }
        }

        /// <summary>
        /// クラスタサービス(e.g. k8s)のWSSのURI
        /// </summary>
        public string WssUri
        {
            get
            {
                return $"wss://{ HostName }:{ PortNo }";
            }
        }
    }
}
