<template>
  <div class="register">
    <h1>Register</h1>

    <br />

    Credentials:<br />
    <input
      name="email"
      type="text"
      v-model="email"
      placeholder="Email" /><br />
    <input
      name="password"
      type="password"
      v-model="password"
      placeholder="Password" /><br />
    <input
      name="confirm"
      type="password"
      v-model="confirmPassword"
      placeholder="Confirm Password" /><br />
    <input
      name="dateofbirth"
      type="text"
      v-model="dob"
      placeholder="Date of Birth (dd/mm/yyyy)" /><br />

    <br /><br />
    Security Questions:<br />
    <input
      name="securityq1"
      type="text"
      v-model="securityQ1"
      placeholder="Security Question 1" /><br />
    <input
      name="securitya1"
      type="text"
      v-model="securityQ1Answer"
      placeholder="Security Answer 1" /><br />

    <br />

    <input
      name="securityq2"
      type="text"
      v-model="securityQ2"
      placeholder="Security Question 2" /><br />
    <input
      name="securitya2"
      type="text"
      v-model="securityQ2Answer"
      placeholder="Security Answer 2" /><br />

    <br />

    <input
      name="securityq3"
      type="text"
      v-model="securityQ3"
      placeholder="Security Question 3" /><br />
    <input
      name="securitya3"
      type="text"
      v-model="securityQ3Answer"
      placeholder="Security Answer 2" /><br />

    <br />

    <button type="submit" v-on:click="submit">Register</button>
  </div>
</template>

<script>
import { register } from '@/services/request';

export default {
  name: 'Register',
  data: () => {
    return {
      email: '',
      password: '',
      confirmPassword: '',
      dob: '',
      securityQ1: '',
      securityQ1Answer: '',
      securityQ2: '',
      securityQ2Answer: '',
      securityQ3: '',
      securityQ3Answer: '',
    }
  },
  methods: {
    submit: function() {
      register({
        email: this.email,
        password: this.password,
        confirmPassword: this.confirmPassword,
        dob: this.dob,
        securityQ1: this.securityQ1,
        securityA1: this.securityA1,
        securityQ2: this.securityQ2,
        securityA2: this.securityA2,
        securityQ3: this.securityQ3,
        securityA3: this.securityA3  
      }).then(() => {
        const params = new URLSearchParams(window.location.search)

        if (params.has('redirect')) {
          window.location.href = decodeURIComponent(params.get('redirect'));
        } else {
          this.$router.push('dashboard');
        }
      })
    }
  }
}
</script>

<style scoped>
  
</style>
