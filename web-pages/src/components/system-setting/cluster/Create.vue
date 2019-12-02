<template>
  <el-dialog class="dialog"
             title="クラスタ登録"
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
                               controls-position="right" style="width:100%;"/>
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
      <el-row class="right-button-group footer">
        <el-button @click="emitCancel">キャンセル</el-button>
        <el-button type="primary" @click="createCluster">登録</el-button>
      </el-row>
    </el-form>
  </el-dialog>
</template>
<script>
  import api from '@/api/v1/api'
  import DisplayError from '@/components/common/DisplayError'

  export default {
    name: 'ClusterCreate',
    components: {
      'pl-display-error': DisplayError
    },
    async created () {
      await this.retrieveData()
    },
    data () {
      return {
        dialogVisible: true,
        error: undefined,
        displayName: undefined,
        hostName: undefined,
        portNo: 6443,
        token: undefined,
        memo: undefined,
        selectedTenants: [], // Selected tenants which can access this node.
        tenants: [], // Tenants to display on a transfer component.
        titles: ['アクセス拒否', 'アクセス許可'], // The title of the transfer component.
        rules: {
          displayName: [{
            required: true,
            trigger: 'blur',
            message: '必須項目です'
          }],
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
          token: [{
            required: true,
            trigger: 'blur',
            message: '必須項目です'
          }]
        }
      }
    },
    methods: {
      async createCluster () {
        let form = this.$refs.createForm
        await form.validate(async (valid) => {
            if (valid) {
              try {
                let params = {
                  displayName: this.displayName,
                  hostName: this.hostName,
                  portNo: this.portNo,
                  memo: this.memo,
                  resourceManageKey: this.token,
                  assignedTenantIds: this.selectedTenants
                }
                await api.cluster.admin.post({model: params})
                this.emitDone()
                this.error = undefined
              } catch (e) {
                this.error = e
              }
            }
          }
        )
      },
      async retrieveData () {
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
      closeDialog (done) {
        done()
        this.emitCancel()
      },
      emitCancel () {
        this.$emit('cancel')
      },
      emitDone () {
        this.showSuccessMessage()
        this.$emit('done')
      }
    }
  }
</script>

<style lang="scss" scoped>
  .dialog /deep/ label {
    font-weight: bold !important
  }

  .dialog /deep/ .el-dialog__title {
    font-size: 24px
  }

  .right-button-group {
    text-align: right;
  }

  .footer {
    padding-top: 40px;
  }

</style>
