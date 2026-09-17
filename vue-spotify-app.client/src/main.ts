import { createApp } from 'vue'
import { createPinia } from 'pinia'
import {Quasar, Loading, Dialog, Notify} from 'quasar'
import quasarLang from "quasar/lang/en-GB"

import '@quasar/extras/material-icons/material-icons.css'

import 'quasar/src/css/index.sass'

import App from './App.vue'
import router from './router'
import { useAuthStore } from './stores/authStore'
import { useTitle } from '@vueuse/core'
import axios from 'axios'

const title = useTitle();

const app = createApp(App)

app.use(createPinia())
app.use(router)
app.use(Quasar, {
  plugins: {
    Dialog,
    Loading,
    Notify
  },
  lang: quasarLang
})

app.mount('#app')

router.beforeEach(async (to) => {
  const authStore = useAuthStore()

  const code = to.query.code?.toString()
  if (code) {
    try {
      const code_verifier = authStore.codeVerifier

      const response = await axios.get(`/api/auth/callback?code=${code}&state=a&redirectpath=${authStore.redirectPath}`)

      //Gets profile
      const profileResponse = await axios.get('/api/auth/me')

      authStore.setUserName(profileResponse.data.DisplayName)
      authStore.setAvatar(profileResponse.data.ProfileImageLink)
      authStore.setLoggedIn(200)

      const url = new URL(window.location.href)
      const searchParams = new URLSearchParams(url.search)
      searchParams.delete('code')
      searchParams.delete('state')
      url.searchParams.delete('code')
      url.searchParams.delete('state')

      title.value = to.meta.title as string || 'Vue Spotify App';
      router.replace(authStore.redirectPath);
    }
    catch (ex) {
      title.value = 'Error';
      const error = ex as AxiosError;
      authStore.setLoggedIn(error.response?.status || 0);
    }


  }
  else{
    title.value = to.meta.title as string || 'Vue Spotify App';
  }

  }
)
