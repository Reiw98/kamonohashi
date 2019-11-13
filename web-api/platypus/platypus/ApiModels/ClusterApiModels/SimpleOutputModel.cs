using Nssol.Platypus.Models;

namespace Nssol.Platypus.ApiModels.ClusterApiModels
{
    /// <summary>
    /// クラスタ情報のうち、コスト最小で取得できる情報だけを保持する
    /// </summary>
    public class SimpleOutputModel : Components.OutputModelBase
    {
        public SimpleOutputModel(Cluster cluster) : base(cluster)
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
