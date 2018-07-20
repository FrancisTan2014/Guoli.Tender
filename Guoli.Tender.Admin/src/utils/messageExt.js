import Vue from 'vue'
import Message from 'element-ui/lib/message'

let defaultDuration = 3500

Vue.prototype.$info = function (content, duration) {
  Message({
    type: 'info',
    message: content,
    duration: duration || defaultDuration
  })
}

Vue.prototype.$success = function (content, duration) {
  Message({
    type: 'success',
    message: content,
    duration: duration || defaultDuration
  })
}

Vue.prototype.$warn = function (content, duration) {
  Message({
    type: 'warn',
    message: content,
    duration: duration || defaultDuration
  })
}

Vue.prototype.$error = function (content, duration) {
  Message({
    type: 'error',
    message: content,
    duration: duration || defaultDuration
  })
}
