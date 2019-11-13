using Nssol.Platypus.Models;

namespace Nssol.Platypus.ApiModels.ClusterApiModels
{
    /// <summary>
    /// クラスタ情報のうち、Indexで表示する最低情報だけを保持する
    /// </summary>
    public class IndexOutputModel : SimpleOutputModel
    {
        public IndexOutputModel(Cluster cluster) : base(cluster)
        {
            HostName = cluster.HostName;
            Memo = cluster.Memo;
        }

        /// <summary>
        /// ホスト名
        /// </summary>
        public string HostName { get; set; }

        /// <summary>
        /// メモ
        /// </summary>
        public string Memo { get; set; }
    }
}
