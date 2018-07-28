import request from '@/utils/request'

import fromTemplate from './apiTemplate'

const controller = 'dict'
const api = fromTemplate(controller)

api.fetchByType = function (type) {
    return request({
        url: `/${controller}/fetchByType`,
        method: 'get',
        data: { type: type }
    })
}

api.fetchTypes = function () {
    return request({
        url: `/${controller}/fetchTypes`,
        method: 'get'
    })
}

api.addType = function (name) {
    return request({
        url: `/${controller}/addType`,
        method: 'post',
        data: { name: name }
    })
}

export default api
