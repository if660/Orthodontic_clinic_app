import { createApp } from 'vue'
import { createPinia } from 'pinia'
import App from './App.vue'
import router from './router'
import { vAutofocus } from './directives/autofocus'
import { initAuthInterceptor } from './services/authService'
import './assets/main.css'

const app = createApp(App)
const pinia = createPinia()

initAuthInterceptor()

app.use(router)
app.use(pinia)
app.directive('autofocus', vAutofocus)

app.mount('#app')
