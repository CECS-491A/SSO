<template>
  <div class="update">
     <h1>Update Password</h1>
    <br />
    {{message}}
    <br /><br />
      <p v-if="errors.length">
        <b>Please correct the following error(s):</b>
          <ul>
            <li v-for="(error, index) in errors" :key="index">
              {{ error }}
            </li>
          </ul>
      </p>
    <div class="oldPassword">
        <input name="oldPassword" type="text" v-model="oldPassword"/>
        <br/>
        <button type="submit" v-on:click="submitOldPassword">Confirm Old Password</button>
    </div>

    <div class="newPassword" v-if="oldPasswordCorrect">
        <input name="newPassword" type="text" v-model="newPassword" />
        <br/>
        Confirm New Password
        <input name="confirmNewPassword" type="text" v-model="confirmNewPassword"/>
        <br />
      <button type="submit" v-on:click="submitNewPassword">Update Password</button>
    </div>
  </div>
</template>

<script>
import axios from 'axios'
export default {
  name: 'UpdatePassword',
  data () {
    return {
      message: 'Enter the new password:',
      errors: [],
      oldPassword: null,
      oldPasswordCorrect: false,
      newPassword: null,
      confirmNewPassword: null
    }
  },
  methods: {
    submitOldPassword: function (e) {
      if (this.oldPassword.length < 12) {
        this.errors.push('Password does not meet minimum length of 12')
      } else if (this.oldPassword.length > 2000) {
        this.errors.push('Password exceeds maximum length of 2000')
      } else {
        axios({
          method: 'POST',
          url: 'api.kfcsso.com/api/user/checkpassword',
          data: {oldPassword: this.$data.oldPassword},
          headers: {
            'Access-Control-Allow-Origin': '*',
            'Access-Control-Allow-Credentials': true
          }
        })
          .then(response => (this.oldPasswordCorrect = response.data))
          .catch(e => { this.errors.push(e) })
        if (!this.oldPasswordCorrect) {
          this.errors.push('Password Incorrect')
        }
      }
    },

    submitNewPassword: function (e) {
      if (this.newPassword.length < 12) {
        this.errors.push('Password does not meet minimum length of 12')
      } else if (this.newPassword.length > 2000) {
        this.errors.push('Password exceeds maximum length of 2000')
      } else if (this.confirmNewPassword !== this.newPassword) {
        this.errors.push('Passwords do not match')
      } else {
        this.errors = []
        this.message = 'Updating Password'
        axios({
          method: 'POST',
          url: 'api.kfcsso.com/api/user/updatpPassword',
          data: {newPassword: this.$data.confirmNewPassword},
          headers: {
            'Access-Control-Allow-Origin': '*',
            'Access-Control-Allow-Credentials': true
          }
        })
      }
    }
  }
}
