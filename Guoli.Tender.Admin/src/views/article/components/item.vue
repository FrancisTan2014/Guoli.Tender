<template>
  <div class="list-item">
    <h3 class="title">
      <a :href="data.SourceUrl" target="_blank" v-html="data.Title"></a>
    </h3>
    <p class="content" v-html="data.ContentWithoutHtml"></p>
    <div class="info">
      <a class="depart" :href="data.DepartmentUrl" target="_blank">来源：{{ data.DepartmentName }}</a>
      <i class="time">发布时间：{{ data.PubTime | time }}</i>
    </div>
  </div>
</template>

<script>
import moment from 'moment'

export default {
  name: 'ListItem',
  componentName: 'ListItem',
  props: {
    data: {
      required: true
    }
  },

  filters: {
    time(s) {
      return moment(s).format('YYYY-MM-DD HH:mm:ss')
    }
  }
}
</script>

<style scoped>

</style>
