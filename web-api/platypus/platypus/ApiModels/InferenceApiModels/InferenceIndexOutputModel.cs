﻿using Nssol.Platypus.Models.TenantModels;
using System.Collections.Generic;

namespace Nssol.Platypus.ApiModels.InferenceApiModels
{
    /// <summary>
    /// 推論履歴のうち、Indexで表示する最低情報だけを保持する
    /// </summary>
    public class InferenceIndexOutputModel : InferenceSimpleOutputModel
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="history">推論履歴</param>
        public InferenceIndexOutputModel(InferenceHistory history) : base(history)
        {
            DataSet = new DataSetApiModels.IndexOutputModel(history.DataSet);
            EntryPoint = history.EntryPoint;
            if (history.ParentMaps != null && history.ParentMaps.Count > 0)
            {
                List<string> parentFullNameList = new List<string>();
                foreach (InferenceHistoryParentMap parentMap in history.ParentMaps)
                {
                    parentFullNameList.Add($"{parentMap.Parent.Id}:{parentMap.Parent.Name}");
                }
                ParentFullNameList = parentFullNameList;
            }
        }

        /// <summary>
        /// データセット
        /// </summary>
        public DataSetApiModels.IndexOutputModel DataSet { get; set; }

        /// <summary>
        /// ジョブ実行コマンド
        /// </summary>
        public string EntryPoint { get; set; }

        /// <summary>
        /// 親学習ジョブ名（表示用）
        /// </summary>
        public List<string> ParentFullNameList { get; set; }

        /// <summary>
        /// 出力値
        /// </summary>
        public string OutputValue { get; set; }
    }
}
