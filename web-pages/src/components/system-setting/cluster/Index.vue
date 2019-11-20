<template>
  <div>
    <h2> クラスタ管理 </h2>
    <el-row :gutter="20">
      <el-col class="right-top-button">
        <el-button @click="openCreateDialog" icon="el-icon-edit-outline" type="primary" plain>
          新規登録
        </el-button>
      </el-col>
    </el-row>
    <el-row>
      <el-table class="data-table pl-index-table" :data="tableData" @row-click="openEditDialog" border>
        <el-table-column prop="id" label="ID" width="120px"/>
        <el-table-column prop="displayName" label="表示名" width="auto"/>
        <el-table-column prop="hostName" label="ホスト名" width="auto"/>
        <el-table-column prop="memo" label="メモ" width="auto"/>
      </el-table>
    </el-row>
    <router-view @cancel="closeDialog" @done="done"></router-view>
  </div>
</template>

<script>
  import api from '@/api/v1/api'

  export default {
    name: 'ClusterIndex',
    title: 'クラスタ管理',
    data () {
      return {
        tableData: []
      }
    },
    async created () {
      await this.retrieveData()
    },
    methods: {
      async retrieveData () {
        let response = await api.cluster.admin.get()
        this.tableData = response.data
      },
      openCreateDialog () {
        this.$router.push('/cluster/create')
      },
      openEditDialog (selectedRow) {
        this.$router.push('/cluster/' + selectedRow.id)
      },
      closeDialog () {
        this.$router.push('/cluster')
      },
      async done () {
        await this.retrieveData()
        this.closeDialog()
        this.showSuccessMessage()
      }
    }
  }
</script>

<style lang="scss" scoped>
  .right-top-button {
    text-align: right;
  }

</style>
