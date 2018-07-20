<template>
  <section class="content-container">

    <div class="mb10">
      <el-button type="primary" icon="plus" @click="showEdit(null)">添加</el-button>
    </div>

    <el-table :data="list" style="width: 100%" :loading="listLoading" border>
      <el-table-column label="序号" type="index" :width="80"></el-table-column>
      <el-table-column prop="Name" label="单位名称" min-width="180"></el-table-column>
      <el-table-column label="列表页地址" min-width="300" show-overflow-tooltip>
        <template slot-scope="scope">
          <a target="_blank" :href="scope.row.ListPageUrl">{{ scope.row.ListPageUrl }}</a>
        </template>
      </el-table-column>
      <el-table-column label="操作">
        <template slot-scope="scope">
          <el-button type="primary" size="small" icon="edit" @click="showEdit(scope.row)">修改</el-button>
          <!-- <el-button type="danger" size="small" icon="delete2">删除</el-button> -->
        </template>
      </el-table-column>
    </el-table>

    <el-dialog :title="dialogTitle" :visible.sync="dialogVisible">
      <el-form :model="model" ref="editForm" :rules="rules" label-width="120px">
        <el-form-item label="单位名称" prop="Name">
          <el-input v-model="model.Name" @keyup.enter.native="editConfirm"></el-input>
        </el-form-item>
        <el-form-item label="列表页地址" prop="ListPageUrl">
          <el-input v-model="model.ListPageUrl" @keyup.enter.native="editConfirm"></el-input>
        </el-form-item>
      </el-form>
      <span slot="footer" class="dialog-footer">
        <el-button @click="dialogVisible = false">取 消</el-button>
        <el-button type="primary" @click="editConfirm">确 定</el-button>
      </span>
    </el-dialog>

  </section>
</template>

<script>
import NProgress from 'nprogress'
import departApi from '@/api/depart'

export default {
  data() {
    return {
      list: [],
      listLoading: false,
      dialogTitle: '添加',
      dialogVisible: false,
      model: {
        Id: 0,
        Name: '',
        ListPageUrl: ''
      },
      rules: {
        Name: [
          { required: true, message: '请输入单位名称' },
          { max: 20, message: '请输入少于20字的名称' },
        ],
        ListPageUrl: [
          { required: true, message: '请输入列表页地址' }
        ]
      }
    }
  },

  methods: {
    loadList() {
      NProgress.start()
      departApi.fetchAll().then(res => {
        NProgress.done()
        this.list = res.data
      })
    },

    showEdit(m) {
      let o = this.model
      if (m) {
        Object.assign(o, m)
        this.dialogTitle = '修改'
      } else {
        o.Id = 0
        o.Name = ''
        o.ListPageUrl = ''
        this.dialogTitle = '添加'
      }
      this.dialogVisible = true
    },

    editConfirm() {
      this.$refs.editForm.validate(valid => {
        if (valid) {
          let m = this.model
          let post = m.Id > 0 ? departApi.update : departApi.add
          post(m).then(res => {
            if (res.code == 200) {
              this.$success(`${this.dialogTitle}成功`)
              this.dialogVisible = false
              this.loadList()
            } else {
              this.$error(res.msg)
            }
          })
        }
      })
    }
  },

  mounted() {
    this.loadList()
  },
};
</script>

<style>
</style>
