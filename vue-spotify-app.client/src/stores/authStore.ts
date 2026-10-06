// Store for user authorisation

import { defineStore } from 'pinia'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    codeVerifier: localStorage.getItem('code_verifier'),
    accessToken: localStorage.getItem('access_token'),
    refreshToken: localStorage.getItem('refresh_token'),
    expiresIn: localStorage.getItem('expires_in'),
    expires: localStorage.getItem('expires'),

    redirectPath: localStorage.getItem('redirect_path') || '/',

    userName: localStorage.getItem('user_name'),
    avatar: localStorage.getItem('avatar_link'),

    loggedIn: localStorage.getItem('logged_in'),
  }),
  actions: {
    getUserName(){
      return this.userName;
    },
    setUserName(value: string) {
      localStorage.setItem('user_name', value)
      this.userName = value
    },
    setAvatar(value: string) {
      localStorage.setItem('avatar_link', value)
      this.avatar = value
    },
    setLoggedIn(value: number) {
      localStorage.setItem('logged_in', value.toString())
      this.loggedIn = value.toString();
    },
    setRedirectPath(value: string) {
      localStorage.setItem('redirect_path', value)
      this.redirectPath = value
    },
    logout() {
      this.codeVerifier = null
      this.accessToken = null
      this.refreshToken = null
      this.expiresIn = null
      this.expires = null
      this.userName = null
      this.avatar = null
      this.loggedIn = "0"
      this.redirectPath = '/'

      localStorage.clear()
    },
  },
})
