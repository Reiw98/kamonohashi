using Nssol.Platypus.Models;

namespace Nssol.Platypus.ApiModels.ClusterApiModels
{
    /// <summary>
    /// クラスタ情報のうち、コスト最小で取得できる情報だけを保持する
    /// </summary>
    public class SimpleOutputModel
    {
        public SimpleOutputModel(Cluster cluster)
        {
            Id = cluster.Id;
            DisplayName = cluster.DisplayName;
        }

        /// <summary>
        /// ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 表示名
        /// </summary>
        public string DisplayName { get; set; }
    }
}
