import { createApp } from 'vue'
import { createPinia } from 'pinia'
import {Quasar, Loading, Dialog, Notify} from 'quasar'
import quasarLang from "quasar/lang/en-GB"

import '@quasar/extras/material-icons/material-icons.css'

import 'quasar/src/css/index.sass'

import App from './App.vue'
import router from './router'
import { useAuthStore } from './stores/authStore'
import axios from 'axios'

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
    const code_verifier = authStore.codeVerifier

    const response = await axios.get(`/auth/callback?code=${code}&state=a`)

    authStore.setAccessToken(response.data.access_token)
    authStore.setRefreshToken(response.data.refresh_token)
    authStore.setExpiresIn(response.data.expires_in)

    //Gets profile
    const profileResponse = await axios.get('https://api.spotify.com/v1/me', {
      headers: {
        Authorization: `Bearer ${response.data.access_token}`,
      },
    })

    authStore.setUserName(profileResponse.data.display_name)
    if (profileResponse.data.images.length > 0) {
      authStore.setAvatar(profileResponse.data.images[0].url)
    } else {
      authStore.setAvatar('')
    }

    const url = new URL(window.location.href)
    url.searchParams.delete('code')
    const updadedUrl = url.search ? url.href : url.href.replace('?', '')
    router.replace('/')
  }

  try {
    const meResponse = await axios.get('auth/me');
    authStore.setLoggedIn(meResponse.status);
}
  catch (ex) {
    const error = ex as AxiosError;
    authStore.setLoggedIn(error.response?.status || 0);


}
}
)
