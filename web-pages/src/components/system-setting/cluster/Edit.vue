<template>
  <el-dialog class="dialog"
             title="クラスタ編集"
             :visible.sync="dialogVisible"
             :before-close="closeDialog"
             :close-on-click-modal="false">
    <el-form ref="createForm" :model="this" :rules="rules">
      <pl-display-error :error="error"/>
      <el-form-item label="表示名" prop="displayName">
        <el-input v-model="displayName"/>
      </el-form-item>
      <el-row>
        <el-col :span="16">
          <el-form-item label="ホスト名" prop="hostName">
            <el-input v-model="hostName"/>
          </el-form-item>
        </el-col>
        <el-col :span="8">
          <el-form-item label="ポート" prop="portNo">
            <el-input-number v-model="portNo" :min="1" :max="65535"
                             controls-position="right" style="width: 100%;"/>
          </el-form-item>
        </el-col>
      </el-row>
      <el-form-item label="トークン" prop="token">
        <el-input type="textarea" v-model="token"/>
      </el-form-item>
      <el-form-item label="メモ">
        <el-input type="textarea" v-model="memo"/>
      </el-form-item>
      <transition name="el-fade-in-linear">
        <el-transfer v-model="selectedTenants" :data="tenants" :titles="titles"/>
      </transition>
      <el-row :gutter="20" class="footer">
        <el-col :span="12">
          <pl-delete-button @delete="deleteCluster"/>
        </el-col>
        <el-col class="right-button-group" :span="12">
          <el-button @click="emitCancel">キャンセル</el-button>
          <el-button type="primary" @click="updateCluster">保存</el-button>
        </el-col>
      </el-row>
    </el-form>
  </el-dialog>
</template>

<script>
  import DisplayError from '@/components/common/DisplayError'
  import DeleteButton from '@/components/common/DeleteButton.vue'
  import api from '@/api/v1/api'

  export default {
    name: 'ClusterEdit',
    components: {
      'pl-display-error': DisplayError,
      'pl-delete-button': DeleteButton
    },
    props: {
      id: String
    },
    data () {
      return {
        dialogVisible: true,
        error: undefined,
        displayName: undefined,
        hostName: undefined,
        portNo: undefined,
        token: undefined,
        memo: undefined,
        selectedTenants: [], // Selected tenants which can access this node.
        tenants: [], // Tenants to display on a transfer component.
        titles: ['アクセス拒否', 'アクセス許可'], // The title of the transfer component.
        rules: {
          hostName: [{
            required: true,
            trigger: 'blur',
            message: '必須項目です'
          }],
          portNo: [{
            required: true,
            trigger: 'blur',
            message: '必須項目です'
          }],
          displayName: [{
            required: true,
            trigger: 'blur',
            message: '必須項目です'
          }],
          token: [{
            required: true,
            trigger: 'blur',
            message: '必須項目です'
          }]
        }
      }
    },
    async created () {
      await this.retrieveData()
    },
    methods: {
      async updateCluster () {
        let form = this.$refs.createForm
        await form.validate(async (valid) => {
          if (valid) {
            try {
              let params = {
                id: this.id,
                model: {
                  displayName: this.displayName,
                  hostName: this.hostName,
                  portNo: this.portNo,
                  memo: this.memo,
                  resourceManageKey: this.token,
                  assignedTenantIds: this.selectedTenants
                }
              }
              await api.cluster.admin.put(params)
              this.emitDone()
              this.error = undefined
            } catch (e) {
              this.error = e
            }
          }
        })
      },
      async retrieveData () {
        let result = (await api.cluster.admin.getById({id: this.id})).data
        this.displayName = result.displayName
        this.hostName = result.hostName
        this.portNo = result.portNo
        this.memo = result.memo
        this.token = result.resourceManageKey
        this.selectedTenants = result.assignedTenants ? result.assignedTenants.map(t => {
          return t.id
        }) : []

        // retrieve tenant to set up a transfer list.
        let allTenants = (await api.tenant.admin.get()).data
        allTenants.forEach(t => {
          if (this.selectedTenants.every(s => s.id !== t.id)) {
            this.tenants.push({
              key: t.id,
              label: t.displayName
            })
          }
        })
      },
      async deleteCluster () {
        try {
          await api.cluster.admin.delete({id: this.id})
          this.emitDone()
          this.error = undefined
        } catch (e) {
          this.error = e
        }
      },
      emitDone () {
        this.showSuccessMessage()
        this.$emit('done')
        this.dialogVisible = false
      },
      emitCancel () {
        this.$emit('cancel')
      },
      closeDialog (done) {
        done()
        this.emitCancel()
      }
    }
  }
</script>

<style lang="scss" scoped>
  .right-button-group {
    text-align: right;
  }

  .dialog /deep/ label {
    font-weight: bold !important
  }

  .dialog /deep/ .el-dialog__title {
    font-size: 24px
  }

  .footer {
    padding-top: 40px;
  }

</style>
