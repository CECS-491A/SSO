import Vue from 'vue'
import VueRouter from 'vue-router'

import Home from '@/views/Home.vue'
import NotFound from '@/views/NotFound.vue'
import AppRegister from '@/views/AppRegister.vue'
import GenerateKey from '@/views/GenerateKey.vue'
import AppDelete from '@/views/AppDelete.vue'
import Login from '@/views/Login.vue'
import Dashboard from '@/views/Dashboard.vue'
import SendResetLink from '@/views/SendResetLink'
import ResetPassword from '@/views/ResetPassword'
import UpdatePassword from '@/views/UpdatePassword'

Vue.use(VueRouter)

let router = new VueRouter({
  routes: [
    {
      path: '/',
      redirect: '/home'
    },
    {
      path: '/home',
      name: 'home',
      component: Home
    },
    {
      path: '/about',
      name: 'about',
      // route level code-splitting
      // this generates a separate chunk (about.[hash].js) for this route
      // which is lazy-loaded when the route is visited.
      component: () => import(/* webpackChunkName: "about" */ './views/About.vue')
    },
    {
      path: '/register',
      name: 'register',
      component: () => import(/* webpackChunkName: "about" */ './views/Register.vue')
    },
    {
      path: '/add',
      name: 'app register',
      component: AppRegister
    },
    {
      path: '/key',
      name: 'api key',
      component: GenerateKey
    },
    {
      path: '/delete',
      name: 'app delete',
      component: AppDelete
    },
    {
      path: '/login',
      name: 'login',
      component: Login
    },
    {
      path: '/dashboard',
      name: 'dashboard',
      component: Dashboard
    },
    {
      path: '/sendresetlink',
      name: 'sendresetLink',
      component: SendResetLink
    },
    {
      path: '/resetpassword/:id',
      name: 'resetpassword',
      component: ResetPassword
    },
    {
      path: '/updatepassword',
      name: 'updatepassword',
      component: UpdatePassword
    },
    {
      path: '*',
      component: NotFound
    }
  ]
})

export default router
