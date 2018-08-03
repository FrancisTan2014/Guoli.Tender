import request from '@/utils/request'

import fromTemplate from './apiTemplate'

const controller = 'keyword'
const api = fromTemplate(controller)

api.fetchList = function (query) {
  return request({
    method: 'get',
    url: `/${controller}/fetchList`,
    params: query
  })
}

export default api
