import request from '@/utils/request'

export default function createApi(controller) {
  return {
    fetchAll() {
      return request({
        url: `/${controller}/fetchAll`,
        method: 'get'
      })
    },

    fetch(id) {
      return request({
        url: `/${controller}/fetch`,
        method: 'get',
        data: { id: id }
      })
    },

    add(data) {
      return request({
        url: `/${controller}/add`,
        method: 'post',
        data: data
      })
    },

    update(data) {
      return request({
        url: `/${controller}/update`,
        method: 'post',
        data: data
      })
    },

    remove(id) {
      return request({
        url: `/${controller}/remove`,
        method: 'post',
        data: { id: id }
      })
    }
  }
}
