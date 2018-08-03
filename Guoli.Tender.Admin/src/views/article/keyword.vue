<template>
  <section class="content-container">

    <div class="mb10">
      <el-button type="primary" icon="el-icon-plus" size="small" @click="showEdit(null)">添加关键词</el-button>
    </div>

    <el-table :data="list" :loading="listLoading" border>
      <el-table-column label="序号" type="index" width="100px"></el-table-column>
      <el-table-column label="关键词" prop="FullKeyword"></el-table-column>
      <el-table-column label="拆分关键词" prop="SplitedKeywords"></el-table-column>
      <el-table-column label="添加时间">
        <template slot-scope="scope">
          <span>{{ scope.row.AddTime | time }}</span>
        </template>
      </el-table-column>
      <el-table-column label="状态">
        <template slot-scope="scope">
          <el-tag :type="scope.row.IsHot ? 'success' : 'default'">{{ scope.row.IsHot ? '已重点关注' : '未重点关注' }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column label="操作">
        <template slot-scope="scope">
          <el-button :type="scope.row.IsHot ? 'default' : 'success'" size="small" @click="onChangeStatus(scope.row)">{{ scope.row.IsHot ? '取消重点关注' : '设置为重点关注' }}</el-button>
        </template>
      </el-table-column>
    </el-table>

    <el-pagination
      class="mt10"
      background
      :total="total"
      :current-page="query.page"
      :page-size="query.size"
      :page-sizes="[10, 20, 50, 100]"
      @current-change="onPageChange"
      @size-change="onSizeChange"
      layout="total, sizes, prev, pager, next"></el-pagination>

    <el-dialog :title="dialogTitle" :visible="dialogVisible" :close-on-click-modal="true">
      <el-form :model="model" ref="editForm" :rules="rules" label-width="120px">
        <el-form-item label="关键词：" prop="FullKeyword">
          <el-input v-model="model.FullKeyword"></el-input>
        </el-form-item>
        <el-form-item label="关键词拆分：" prop="SplitedKeywords">
          <el-input v-model="model.SplitedKeywords" placeholder="请务必以逗号分隔开"></el-input>
        </el-form-item>
        <el-form-item label="是否重点关注：">
          <el-switch
            v-model="model.IsHot"
            active-text="重点关注"
            inactive-text="非重点关注">
          </el-switch>
        </el-form-item>
      </el-form>
      <span slot="footer" class="dialog-footer">
        <el-button @click="dialogVisible = false">取 消</el-button>
        <el-button type="primary" @click="onEditConfirm" :loading="postPendding">确 定</el-button>
      </span>
    </el-dialog>

  </section>
</template>

<script>
import moment from 'moment'
import NProgress from 'nprogress'
import keywordApi from '@/api/keyword'

let emptyModel = {
  Id: 0,
  FullKeyword: '',
  SplitedKeywords: '',
  IsHot: true,
}

export default {
  data() {
    return {
      list: [],
      listLoading: false,
      total: 0,
      query: {
        page: 1,
        size: 10,
      },

      dialogTitle: '添加',
      dialogVisible: false,
      postPendding: false,
      model: Object.assign({}, emptyModel),
      rules: {
        FullKeyword: [
          { required: true, message: '关键词不能为空' },
          { max: 50, message: '关键词长度不能超过50个字' }
        ]
      },
    }
  },

  filters: {
    time(t) {
      return moment(t).format('YYYY-MM-DD HH:mm:ss')
    }
  },

  methods: {
    loadList() {
      this.listLoading = true
      NProgress.start()
      keywordApi.fetchList(this.query).then(res => {
        NProgress.done()
        this.listLoading = false
        this.list = res.data.list
        this.total = res.data.total
      })
    },

    showEdit(model) {
      if (model) {
        Object.assign(this.model, model)
        this.dialogTitle = '修改'
      } else {
        Object.assign(this.model, emptyModel)
        this.dialogTitle = '添加'
      }
      this.dialogVisible = true
    },

    onPageChange() {
      this.loadList()
    },

    onSizeChange() {
      this.loadList()
    },

    onChangeStatus(model) {

    },

    onEditConfirm() {
      this.$refs.editForm.validate(valid => {
        if (valid) {
          let m = Object.assign({}, this.model)
          // replace the Chinese comma to English comma
          m.SplitedKeywords = m.SplitedKeywords.replace('，', ',')

          let postMethod = m.Id > 0 ? keywordApi.update : keywordApi.add

          this.postPendding = true
          NProgress.start()
          postMethod(m).then(res => {
            NProgress.done()
            this.postPendding = false
            this.dialogVisible = false

            if (res.code == 200) {
              this.$success(`${this.dialogTitle}成功`)
              this.loadList()
            } else {
              this.$error(`${this.dialogTitle}失败，请稍后重试`)
            }
          })
        }
      })
    },

  },

  created() {
    this.loadList()
  }
}
</script>

<style>

</style>
