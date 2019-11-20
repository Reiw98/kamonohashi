﻿using Nssol.Platypus.DataAccess.Core;
using Nssol.Platypus.DataAccess.Repositories.Interfaces;
using Nssol.Platypus.DataAccess.Repositories.Interfaces.TenantRepositories;
using Nssol.Platypus.Infrastructure;
using Nssol.Platypus.Infrastructure.Types;
using Nssol.Platypus.Logic.Interfaces;
using Nssol.Platypus.Models;
using Nssol.Platypus.Models.TenantModels;
using System;
using System.Threading.Tasks;

namespace Nssol.Platypus.Logic
{
    /// <summary>
    /// ノートブックロジッククラス
    /// </summary>
    /// <seealso cref="Nssol.Platypus.Logic.Interfaces.INotebookLogic" />
    public class NotebookLogic : PlatypusLogicBase, INotebookLogic
    {
        private readonly INotebookHistoryRepository notebookHistoryRepository;
        private readonly IClusterRepository clusterRepository;
        private readonly IClusterManagementLogic clusterManagementLogic;
        private readonly IUnitOfWork unitOfWork;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public NotebookLogic(
            INotebookHistoryRepository notebookHistoryRepository,
            IClusterRepository clusterRepository,
            IClusterManagementLogic clusterManagementLogic,
            IUnitOfWork unitOfWork,
            ICommonDiLogic commonDiLogic) : base(commonDiLogic)
        {
            this.notebookHistoryRepository = notebookHistoryRepository;
            this.clusterRepository = clusterRepository;
            this.clusterManagementLogic = clusterManagementLogic;
            this.unitOfWork = unitOfWork;
        }

        /// <summary>
        /// ノートブック履歴コンテナを削除し、ステータスを変更する。
        /// </summary>
        /// <param name="notebookHistory">対象ノートブック履歴</param>
        /// <param name="status">変更後のステータス</param>
        /// <param name="force">他テナントに対する変更を許可するか</param>
        public async Task ExitAsync(NotebookHistory notebookHistory, ContainerStatus status, bool force)
        {
            // コンテナの生存確認
            if (notebookHistory.GetStatus().Exist())
            {
                // クラスタが設定されている場合、クラスタ情報を取得する
                Cluster cluster = null;
                if (notebookHistory.ClusterId.HasValue)
                {
                    cluster = await clusterRepository.GetByIdAsync(notebookHistory.ClusterId.Value);
                }

                var info = await clusterManagementLogic.GetContainerDetailsInfoAsync(notebookHistory.Key, CurrentUserInfo.SelectedTenant.Name, cluster, force);

                // コンテナ削除の前に、DBの更新を先に実行
                await notebookHistoryRepository.UpdateStatusAsync(notebookHistory.Id, status, DateTime.Now, force);

                // 実コンテナ削除の結果は確認せず、DBの更新を確定する（コンテナがいないなら、そのまま消しても問題ない想定）
                unitOfWork.Commit();

                if (info.Status.Exist())
                {
                    // 再確認してもまだ存在していたら、コンテナ削除

                    await clusterManagementLogic.DeleteContainerAsync(
                        ContainerType.Notebook, notebookHistory.Key, CurrentUserInfo.SelectedTenant.Name, cluster, force);
                }
            }
            else
            {
                await notebookHistoryRepository.UpdateStatusAsync(notebookHistory.Id, status, force);

                // DBの更新を確定する
                unitOfWork.Commit();
            }
        }
    }
}
