<template>
  <section class="content-container">

    <div class="mb10">
      <el-select v-model="selectedDepart" @change="onDepartChange" placeholder="单位">
        <el-option
          v-for="d in departs"
          :key="d.Id"
          :label="d.Name"
          :value="d.Id">
        </el-option>
      </el-select>
      <el-select v-model="selectedStatus" @change="onStatusChange" placeholder="状态">
        <el-option
          v-for="s in status"
          :key="s.value"
          :label="s.label"
          :value="s.value">
        </el-option>
      </el-select>
    </div>

    <el-table :data="list" :loading="listLoading" border>
      <el-table-column label="序号" type="index" width="100px"></el-table-column>
      <el-table-column label="标题" min-width="400" prop="Title" show-overflow-tooltip></el-table-column>
      <!-- <el-table-column label="概要" min-width="250" prop="Summary" show-overflow-tooltip></el-table-column> -->
      <el-table-column label="单位" prop="DepartmentName"></el-table-column>
      <el-table-column label="发布时间">
        <template slot-scope="scope">
          <span>{{ scope.row.PubTime | time }}</span>
        </template>
      </el-table-column>
      <el-table-column label="原文地址">
        <template slot-scope="scope">
          <el-button type="text">
            <a :href="scope.row.SourceUrl" target="_blank">查看原文</a>
          </el-button>
        </template>
      </el-table-column>
      <el-table-column label="状态">
        <template slot-scope="scope">
          <el-tag :type="scope.row.HasRead ? 'info' : 'danger'">{{ scope.row.HasRead ? '已读' : '未读' }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column label="操作">
        <template slot-scope="scope">
          <el-button :type="scope.row.HasRead ? 'default' : 'primary'" size="small" @click="onMarkRead(scope.row)">{{ scope.row.HasRead ? '标为未读' : '标为已读' }}</el-button>
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

  </section>
</template>

<script>
import moment from 'moment'
import NProgress from 'nprogress'
import departApi from '@/api/depart'
import articleApi from '@/api/article'

export default {
  data() {
    return {
      list: [],
      listLoading: false,
      total: 0,
      query: {
        page: 1,
        size: 20,

      },

      departs: [],
      selectedDepart: '',
      status: [
        { value: -1, label: '所有状态' },
        { value: 0, label: '未读' },
        { value: 1, label: '已读' },
      ],
      selectedStatus: '',

      autoHandler: 0,
      autoTimespan: 30 * 1000,
    }
  },

  filters: {
    time(s) {
      return moment(s).format('YYYY-MM-DD HH:mm:ss')
    }
  },

  methods: {
    loadDeparts() {
      departApi.fetchAll().then(res => {
        this.departs = res.data
        this.departs.splice(0, 0, {
          Id: 0,
          Name: '所有铁路局'
        })
      })
    },

    loadList() {
      NProgress.start()
      articleApi.fetchList(this.query).then(res => {
        NProgress.done()
        this.total = res.data.total
        this.list = res.data.list
        this.setArticleList()
      })
    },

    setArticleList() {
      this.list.forEach(a => {
        var d = this.departs.find(d => d.Id == a.DepartmentId)
        a.DepartmentName = d.Name
      })
    },

    autoLoad() {
      let self = this
      self.autoHandler = setInterval(() => {
        self.loadList()
      }, self.autoTimespan)
    },

    onDepartChange(departId) {
      this.query.departId = departId
      this.loadList()
    },

    onPageChange(page) {
      this.query.page = page
      this.loadList()
    },

    onSizeChange(size) {
      this.query.size = size
      this.loadList()
    },

    onStatusChange(status) {
      this.query.status = status
      this.loadList()
    },

    onMarkRead(model) {
      articleApi.hasRead(model.Id, !model.HasRead).then(res => {
        if (res.code == 200) {
          model.HasRead = !model.HasRead
          this.$success('标记成功')
        } else {
          this.$error('标记失败，请稍后重试')
        }
      })
    }
  },

  mounted() {
    let self = this
    Promise.all([this.loadDeparts(), this.loadList()])
      .then(function (values) {
        self.setArticleList()
      })

    this.autoLoad()
  },
}
</script>

<style>

</style>
