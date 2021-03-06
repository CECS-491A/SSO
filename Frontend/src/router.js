import Vue from 'vue'
import VueRouter from 'vue-router'
import Home from '@/views/Home.vue'
import NotFound from '@/views/NotFound.vue'
import AppRegister from '@/views/AppRegister.vue'
import GenerateKey from '@/views/GenerateKey.vue'
import AppDelete from '@/views/AppDelete.vue'
import Login from '@/views/Login.vue'
import Dashboard from '@/views/Dashboard.vue'
import UpdatePassword from '@/views/UpdatePassword.vue'
import SendResetLink from '@/views/SendResetLink.vue'
import ResetPassword from '@/views/ResetPassword.vue'
import Logout from '@/views/Logout.vue'
import DeleteAccount from '@/views/DeleteAccount.vue'
import AccountSettings from '@/views/AccountSettings.vue'
import Landing from '@/views/Landing.vue'

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
      path: '/legal',
      name: 'legal',
      component: () => import(/* webpackChunkName: "about" */ './views/Legal.vue')
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
      path: '/updatepassword',
      name: 'updatepassword',
      component: UpdatePassword
    },
    {
      path: '/sendresetlink',
      name: 'sendresetlink',
      component: SendResetLink
    },
    {
      path: '/resetpassword/:id',
      name: 'resetpassword',
      component: ResetPassword
    },
    {
      path: '/logout',
      name: 'Logout',
      component: Logout
    },
    {
      path: '/deleteaccount',
      name: 'deleteaccount',
      component: DeleteAccount
    },
    {
      path: '/accountsettings',
      name: 'accountsettings',
      component: AccountSettings
    },
    {
      path: '/landing',
      name: 'landing',
      component: Landing,
    },
    {
      path: '*',
      component: NotFound
    }
  ]
})

router.beforeEach((to, from, next) => {
  if((to.fullPath === '/dashboard' || to.fullPath === '/home') && from.fullPath !== '/login'){
    if(!localStorage.getItem('token')){
      next('/login');
    }
    else{
      next();
    }
  }
  else if(from.fullPath === '/login' && to.fullPath === '/dashboard'){
    if(!localStorage.getItem('token')){
      next('/home');
      alert('Please login');
    }
  }

  next();
})

export default router
