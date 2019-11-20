﻿using Microsoft.AspNetCore.Http;
using Nssol.Platypus.Infrastructure;
using Nssol.Platypus.Infrastructure.Infos;
using Nssol.Platypus.Infrastructure.Types;
using Nssol.Platypus.Models;
using Nssol.Platypus.Models.TenantModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nssol.Platypus.Logic.Interfaces
{
    /// <summary>
    /// クラスタ管理・コンテナ管理を行うロジックのインターフェース
    /// </summary>
    public interface IClusterManagementLogic
    {
        #region コンテナ管理

        /// <summary>
        /// 新規に前処理用コンテナを作成する。
        /// </summary>
        /// <returns>作成したコンテナのステータス</returns>
        Task<Result<ContainerInfo, string>> RunPreprocessingContainerAsync(PreprocessHistory preprocessHistory);

        /// <summary>
        /// 新規にTensorBoard表示用のコンテナを作成する。
        /// </summary>
        /// <param name="trainingHistory">対象の学習履歴</param>
        /// <returns>作成したコンテナのステータス</returns>
        Task<ContainerInfo> RunTensorBoardContainerAsync(TrainingHistory trainingHistory);

        /// <summary>
        /// 新規に画像認識の訓練用コンテナを作成する。
        /// </summary>
        /// <returns>作成したコンテナのステータス</returns>
        Task<Result<ContainerInfo, string>> RunTrainContainerAsync(TrainingHistory trainHistory);

        /// <summary>
        /// 新規に画像認識の推論用コンテナを作成する。
        /// </summary>
        /// <returns>作成したコンテナのステータス</returns>
        Task<Result<ContainerInfo, string>> RunInferenceContainerAsync(InferenceHistory inferenceHistory);

        /// <summary>
        /// 新規にノートブック用コンテナを作成する。
        /// </summary>
        /// <returns>作成したコンテナのステータス</returns>
        Task<Result<ContainerInfo, string>> RunNotebookContainerAsync(NotebookHistory notebookHistory);

        /// <summary>
        /// 全コンテナの情報を取得する
        /// </summary>
        /// <param name="cluster">オンプレ環境以外のクラスタ情報（オンプレの場合はnull）</param>
        Task<Result<IEnumerable<ContainerDetailsInfo>, ContainerStatus>> GetAllContainerDetailsInfosAsync(Cluster cluster);

        /// <summary>
        /// 特定のテナントに紐づいた全コンテナの情報を取得する
        /// </summary>
        /// <param name="tenantName">テナント名</param>
        /// <param name="cluster">オンプレ環境以外のクラスタ情報（オンプレの場合はnull）</param>
        Task<Result<IEnumerable<ContainerDetailsInfo>, ContainerStatus>> GetAllContainerDetailsInfosAsync(string tenantName, Cluster cluster);

        /// <summary>
        /// 指定したコンテナのエンドポイント付きの情報をクラスタ管理サービスに問い合わせる。
        /// </summary>
        /// <param name="containerName">コンテナ名</param>
        /// <param name="tenantName">テナント名</param>
        /// <param name="cluster">オンプレ環境以外のクラスタ情報（オンプレの場合はnull）</param>
        /// <param name="force">Admin権限で実行するか</param>
        Task<ContainerEndpointInfo> GetContainerEndpointInfoAsync(string containerName, string tenantName, Cluster cluster, bool force);

        /// <summary>
        /// 指定したコンテナの詳細情報をクラスタ管理サービスに問い合わせる。
        /// </summary>
        /// <param name="containerName">コンテナ名</param>
        /// <param name="tenantName">テナント名</param>
        /// <param name="cluster">オンプレ環境以外のクラスタ情報（オンプレの場合はnull）</param>
        /// <param name="force">Admin権限で実行するか</param>
        Task<ContainerDetailsInfo> GetContainerDetailsInfoAsync(string containerName, string tenantName, Cluster cluster, bool force);

        /// <summary>
        /// 指定したコンテナのステータスをクラスタ管理サービスに問い合わせる。
        /// </summary>
        /// <param name="containerName">コンテナ名</param>
        /// <param name="tenantName">テナント名</param>
        /// <param name="cluster">オンプレ環境以外のクラスタ情報（オンプレの場合はnull）</param>
        /// <param name="force">Admin権限で実行するか</param>
        Task<ContainerStatus> GetContainerStatusAsync(string containerName, string tenantName, Cluster cluster, bool force);

        /// <summary>
        /// 指定したTensorBoardコンテナのステータスをクラスタ管理サービスに問い合わせ、結果でDBを更新する。
        /// </summary>
        Task<ContainerStatus> SyncContainerStatusAsync(TensorBoardContainer container, bool force);

        /// <summary>
        /// 指定したコンテナを削除する。
        /// 対象コンテナが存在しない場合はエラーになる。
        /// </summary>
        /// <param name="type">コンテナ種別</param>
        /// <param name="containerName">コンテナ名</param>
        /// <param name="tenantName">テナント名</param>
        /// <param name="cluster">オンプレ環境以外のクラスタ情報（オンプレの場合はnull）</param>
        /// <param name="force">Admin権限で実行するか</param>
        Task<bool> DeleteContainerAsync(ContainerType type, string containerName, string tenantName, Cluster cluster, bool force);

        /// <summary>
        /// 指定したコンテナのログを取得する
        /// </summary>
        /// <param name="containerName">コンテナ名</param>
        /// <param name="tenantName">テナント名</param>
        /// <param name="cluster">オンプレ環境以外のクラスタ情報（オンプレの場合はnull）</param>
        /// <param name="force">Admin権限で実行するか</param>
        Task<Result<System.IO.Stream, ContainerStatus>> DownloadLogAsync(string containerName, string tenantName, Cluster cluster, bool force);

        /// <summary>
        /// 指定したテナントのイベントを取得する
        /// </summary>
        /// <param name="tenant">テナント情報</param>
        /// <param name="cluster">オンプレ環境以外のクラスタ情報（オンプレの場合はnull）</param>
        /// <param name="force">Admin権限で実行するか</param>
        Task<Result<IEnumerable<ContainerEventInfo>, ContainerStatus>> GetEventsAsync(Tenant tenant, Cluster cluster, bool force);

        /// <summary>
        /// 指定したコンテナのイベントを取得する
        /// </summary>
        /// <param name="tenant">テナント情報</param>
        /// <param name="containerName">コンテナ名</param>
        /// <param name="cluster">オンプレ環境以外のクラスタ情報（オンプレの場合はnull）</param>
        /// <param name="force">Admin権限で実行するか</param>
        /// <param name="errorOnly">エラー情報だけか否か</param>
        Task<Result<IEnumerable<ContainerEventInfo>, ContainerStatus>> GetEventsAsync(Tenant tenant, string containerName, Cluster cluster, bool force, bool errorOnly);
        #endregion

        #region クラスタ管理

        /// <summary>
        /// 全ノード情報を取得する。
        /// 取得失敗した場合はnullが返る。
        /// </summary>
        Task<IEnumerable<NodeInfo>> GetAllNodesAsync();

        ///// <summary>
        ///// 現在のアクセスユーザが利用可能なノード一覧を取得する
        ///// </summary>
        //void GetAccessibleNods();

        ///// <summary>
        ///// 指定したテナントに紐づく管理イベントを取得する
        ///// </summary>
        //void GetEvents(string tenantName);

        /// <summary>
        /// ノード単位のパーティションリストを取得する
        /// </summary>
        Task<Result<Dictionary<string, string>, string>> GetNodePartitionMapAsync();

        /// <summary>
        /// パーティションを更新する
        /// </summary>
        /// <param name="nodeName">ノード名</param>
        /// <param name="labelValue">ノード値</param>
        /// <returns>更新結果、更新できた場合、true</returns>
        Task<bool> UpdatePartitionLabelAsync(string nodeName, string labelValue);

        /// <summary>
        /// TensorBoardの実行可否設定を更新する
        /// </summary>
        /// <param name="nodeName">ノード名</param>
        /// <param name="enabled">実行可否</param>
        /// <returns>更新結果、更新できた場合、true</returns>
        Task<bool> UpdateTensorBoardEnabledLabelAsync(string nodeName, bool enabled);

        /// <summary>
        /// Notebookの実行可否設定を更新する
        /// </summary>
        /// <param name="nodeName">ノード名</param>
        /// <param name="enabled">実行可否</param>
        /// <returns>更新結果、更新できた場合、true</returns>
        Task<bool> UpdateNotebookEnabledLabelAsync(string nodeName, bool enabled);

        /// <summary>
        /// 指定されたテナントのクォータ設定をクラスタに反映させる。
        /// </summary>
        /// <returns>更新結果、更新できた場合、true</returns>
        Task<bool> SetQuotaAsync(Tenant tenant);
        #endregion

        #region 権限管理

        /// <summary>
        /// クラスタ管理サービス上で、指定したユーザ＆テナントにコンテナレジストリを登録する。
        /// idempotentを担保。
        /// </summary>
        Task<bool> RegistRegistryToTenantAsync(string selectedTenantName, UserTenantRegistryMap userRegistryMap);

        /// <summary>
        /// 指定したテナントを作成する。
        /// 既にある場合は何もしない。
        /// </summary>
        /// <param name="tenantName">テナント名</param>
        /// <param name="cluster">オンプレ環境以外のクラスタ情報（オンプレの場合はnull）</param>
        Task<bool> RegistTenantAsync(string tenantName, Cluster cluster);

        /// <summary>
        /// ログイン中のユーザ＆テナントに対する、クラスタ管理サービスにアクセスするためのトークンを取得する。
        /// 存在しない場合、新規に作成する。
        /// </summary>
        /// <param name="cluster">オンプレ環境以外のクラスタ情報（オンプレの場合はnull）</param>
        Task<string> GetUserAccessTokenAsync(Cluster cluster);

        /// <summary>
        /// 指定したテナントを抹消(削除)する。
        /// </summary>
        /// <param name="tenantName">テナント名</param>
        /// <param name="cluster">オンプレ環境以外のクラスタ情報（オンプレの場合はnull）</param>
        Task<bool> EraseTenantAsync(string tenantName, Cluster cluster);

        #endregion

        #region WebSocket通信
        /// <summary>
        /// ブラウザとのWebSocket接続および、KubernetesとのWebSocket接続を確立する。
        /// そしてブラウザからのメッセージを待機し、メッセージを受信した際にはその内容をKubernetesに送信する。
        /// </summary>
        Task ConnectKubernetesWebSocketAsync(HttpContext context);
        #endregion
    }
}
