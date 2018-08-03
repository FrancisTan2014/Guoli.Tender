<template>
  <section class="content-container">

    <div class="mb10">

      <el-input class="search-input" v-model="query.keyword" placeholder="输入关键字搜索" prefix-icon="el-icon-search" @keyup.native="onSearchTitleChange"></el-input>
      <el-select v-model="selectedDepart" @change="onDepartChange" placeholder="单位">
        <el-option
          v-for="d in departs"
          :key="d.Id"
          :label="d.Name"
          :value="d.Id">
        </el-option>
      </el-select>
      <!-- <el-select v-model="selectedStatus" @change="onStatusChange" placeholder="状态">
        <el-option
          v-for="s in status"
          :key="s.value"
          :label="s.label"
          :value="s.value">
        </el-option>
      </el-select> -->
      <!-- <el-date-picker
          v-model="query.day"
          align="right"
          type="date"
          placeholder="选择日期"
          @change="onSearchDayChange"
          :picker-options="pickerOptions">
      </el-date-picker> -->
    </div>

    <!-- <el-table :data="list" :loading="listLoading" border>
      <el-table-column label="序号" type="index" width="100px"></el-table-column>
      <el-table-column label="标题" min-width="400" prop="Title" show-overflow-tooltip></el-table-column>
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
    </el-table> -->

    <ListItem v-for="a in list" v-bind:key="a.Id" :data="a"></ListItem>

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
import _ from "underscore";
import moment from "moment";
import NProgress from "nprogress";
import departApi from "@/api/depart";
import articleApi from "@/api/article";
import ListItem from "./components";

export default {
  data() {
    return {
      list: [],
      listLoading: false,
      total: 0,
      query: {
        page: 1,
        size: 20,
        day: "",
        keyword: ""
      },

      departs: [],
      selectedDepart: "",
      status: [
        { value: -1, label: "所有状态" },
        { value: 0, label: "未读" },
        { value: 1, label: "已读" }
      ],
      selectedStatus: "",

      autoHandler: 0,
      autoTimespan: 30 * 1000,

      pickerOptions: {
        disabledDate(time) {
          return time.getTime() > Date.now();
        },
        shortcuts: [
          {
            text: "今天",
            onClick(picker) {
              picker.$emit("pick", new Date());
            }
          },
          {
            text: "昨天",
            onClick(picker) {
              const date = new Date();
              date.setTime(date.getTime() - 3600 * 1000 * 24);
              picker.$emit("pick", date);
            }
          },
          {
            text: "一周前",
            onClick(picker) {
              const date = new Date();
              date.setTime(date.getTime() - 3600 * 1000 * 24 * 7);
              picker.$emit("pick", date);
            }
          }
        ]
      }
    };
  },

  filters: {
    time(s) {
      return moment(s).format("YYYY-MM-DD HH:mm:ss");
    }
  },

  methods: {
    loadDeparts() {
      departApi.fetchAll().then(res => {
        this.departs = res.data;
        this.departs.splice(0, 0, {
          Id: 0,
          Name: "所有铁路局"
        });
      });
    },

    loadList() {
      NProgress.start();
      articleApi.fetchList(this.query).then(res => {
        NProgress.done();
        this.total = res.data.total;
        this.list = res.data.list;
        this.setArticleList();
      });
    },

    search() {
      this.query.page = 1;
      this.loadList();
    },

    debounceLoadList: null,

    setArticleList() {
      this.list.forEach(a => {
        var d = this.departs.find(d => d.Id == a.DepartmentId);
        a.DepartmentName = d.Name;
        a.DepartmentUrl = d.ListPageUrl;
      });
    },

    autoLoad() {
      let self = this;
      self.autoHandler = setInterval(() => {
        self.loadList();
      }, self.autoTimespan);
    },

    onDepartChange(departId) {
      this.query.departId = departId;
      this.query.page = 1;
      this.loadList();
    },

    onPageChange(page) {
      this.query.page = page;
      this.loadList();
    },

    onSizeChange(size) {
      this.query.size = size;
      this.loadList();
    },

    onStatusChange(status) {
      this.query.page = 1;
      this.query.status = status;
      this.loadList();
    },

    onSearchTitleChange() {
      this.query.page = 1;
      this.debounceLoadList();
    },

    onSearchDayChange() {
      this.query.page = 1;
      this.loadList();
    },

    onMarkRead(model) {
      articleApi.hasRead(model.Id, !model.HasRead).then(res => {
        if (res.code == 200) {
          model.HasRead = !model.HasRead;
          this.$success("标记成功");
        } else {
          this.$error("标记失败，请稍后重试");
        }
      });
    }
  },

  mounted() {
    let self = this;
    Promise.all([this.loadDeparts(), this.loadList()]).then(function(values) {
      self.setArticleList();
    });

    this.autoLoad();

    this.debounceLoadList = _.debounce(this.loadList, 300);
  }
};
</script>

<style lang="scss">
.el-input {
  width: 180px;
}
.search-input {
  width: 300px;
}
.es-highlight {
  color: red !important;
}
.list-item {
  max-width: 65rem;
  font-size: 1rem;
  padding: 0.8rem 0 0.2rem;
  text-align: left;
  line-height: 1.5rem;

  .title {
    a {
      color: #1075d4;
    }
  }

  .content {
    font-size: 0.875rem;
    color: #353535;
    font-weight: 400;
    max-height: 6rem;
    overflow: hidden;
  }
  .info {
    margin-top: 0.4rem;
    font-weight: 400;
    font-size: 0.75rem;
    color: #888;

    i {
      margin-left: 1rem;
      font-style: normal;
    }

    a {
      &:hover {
        color: #1075d4;
      }
    }
  }
}
</style>
