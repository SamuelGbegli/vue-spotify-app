// Store for user authorisation

import { defineStore } from 'pinia'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    codeVerifier: localStorage.getItem('code_verifier'),
    accessToken: localStorage.getItem('access_token'),
    refreshToken: localStorage.getItem('refresh_token'),
    expiresIn: localStorage.getItem('expires_in'),
    expires: localStorage.getItem('expires'),

    userName: localStorage.getItem('user_name'),
    avatar: localStorage.getItem('avatar_link'),

    loggedIn: localStorage.getItem('logged_in'),
  }),
  actions: {
    setCodeVerifier(value: string) {
      localStorage.setItem('code_verifier', value)
      this.codeVerifier = value
    },
    setAccessToken(value: string) {
      localStorage.setItem('access_token', value)
      this.accessToken = value
    },
    setRefreshToken(value: string) {
      localStorage.setItem('refresh_token', value)
      this.refreshToken = value
    },
    setExpiresIn(value: number) {
      localStorage.setItem('expires_in', value.toString())
      this.expiresIn = value.toString()

      const now = new Date()
      const expiry = new Date(now.getTime() + value * 1000)
      this.expires = expiry.toString()
      localStorage.setItem('expires', expiry.toString())
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
    logout() {
      this.codeVerifier = null
      this.accessToken = null
      this.refreshToken = null
      this.expiresIn = null
      this.expires = null
      this.userName = null
      this.avatar = null
      this.loggedIn = "0"

      localStorage.clear()
    },
  },
})
