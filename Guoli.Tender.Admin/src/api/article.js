import request from '@/utils/request'

import fromTemplate from './apiTemplate'

const controller = 'article'
const api = fromTemplate(controller)

api.fetchList = function (query) {
  console.log(query)
  return request({
    method: 'get',
    url: `/${controller}/fetchList`,
    params: query
  })
}

api.hasRead = function (id, status) {
  return request({
    method: 'post',
    url: `/${controller}/hasRead`,
    data: {
      id: id,
      status: status
    }
  })
}

export default api
