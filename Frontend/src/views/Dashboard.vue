<template>
  <div>
    <v-card lg12>
      <v-layout row wrap>
        <v-flex lg9>
          <h1 id="appPortal">Application Portal</h1>
        </v-flex>
        <v-flex lg3 id="appFilters">
          <v-select @change="filterApps" :items="sortBy" label="Sort By"></v-select>
        </v-flex>
      </v-layout>

      <v-container fluid grid-list-md>
        <v-layout row wrap>
          <v-flex xs12 md6 lg6 v-for="(app, index) in applications" :key="index">
            <!-- The card that shows up if the app is under maintenance -->
            <v-card v-if="app.UnderMaintenance" class="transparent">
              <v-card-title primary-title>
                <!-- If there is no logo, then a default image will be shown -->
                <img v-if="app.LogoUrl === null" src="@/assets/no-image-icon.png">
                <img v-else :src="app.LogoUrl">

                <div class="content">
                  <!-- Launching to an app can be done by clicking the app title -->
                  <h3 class="headline mb-0">
                    <strong class="truncate">
                      {{ app.Title }}
                      <v-tooltip right>
                        <template v-slot:activator="{ on }">
                          <v-icon color="orange" large right v-on="on">warning</v-icon>
                        </template>
                        <span>Under Maintenance</span>
                      </v-tooltip>
                    </strong>
                  </h3>
                </div>

                <read-more
                  v-if="app.Description === null"
                  more-str="read more"
                  :text="defaultDescription"
                  less-str="read less"
                  :max-chars="175"
                ></read-more>
                <!-- Allows expansion or shrinkage of app description -->
                <read-more
                  v-else
                  more-str="read more"
                  :text="app.Description"
                  less-str="read less"
                  :max-chars="175"
                ></read-more>
              </v-card-title>
            </v-card>

            <!-- The card that shows up if the app is not under maintenance -->
            <v-card v-else hover>
              <v-card-title primary-title>
                <!-- If there is no logo, then a default image will be shown -->
                <img
                  v-if="app.LogoUrl === null"
                  src="@/assets/no-image-icon.png"
                  @click="launchLoading = true; launch(app.Id, app)"
                >
                <img v-else :src="app.LogoUrl" @click="launchLoading = true; launch(app.Id)">
                <div id="content" v-if="app.UnderMaintenance">
                  <!-- Launching to an app can be done by clicking the app title -->
                  <h3 class="headline mb-0" row wrap>
                    <strong class="truncate">
                      {{ app.Title }}
                      <br>
                    </strong>
                  </h3>

                  <p>{{app.UnderMaintenance}}</p>
                </div>
                <div class="content" v-else>
                  <!-- Launching to an app can be done by clicking the app title -->
                  <h3
                    id="launchable"
                    class="headline mb-0"
                    @click="launchLoading = true; launch(app.Id, app)"
                  >
                    <strong>{{ app.Title }}</strong>
                  </h3>
                </div>

                <read-more
                  v-if="app.Description === null"
                  more-str="read more"
                  :text="defaultDescription"
                  less-str="read less"
                  :max-chars="175"
                ></read-more>
                <!-- Allows expansion or shrinkage of app description -->
                <read-more
                  v-else
                  more-str="read more"
                  :text="app.Description"
                  less-str="read less"
                  :max-chars="175"
                ></read-more>
              </v-card-title>
            </v-card>
            <!-- Loads only if app is in progress of launching -->
            <div v-if="launchLoading">
              <Loading :dialog="launchLoading"/>
            </div>
          </v-flex>
        </v-layout>
      </v-container>
    </v-card>

    <v-alert :value="error" type="error" transition="scale-transition">{{error}}</v-alert>
  </div>
</template>

<script>
import Vue from "vue";
import Loading from "@/components/Dialogs/Loading.vue";
import { signLaunch, submitLaunch } from "@/services/request";
import { apiURL } from "@/const.js";
import axios from "axios";
import ReadMore from "vue-read-more";

Vue.use(ReadMore);

export default {
  components: { Loading },
  data() {
    return {
      applications: [],
      sortBy: [
        "Alphabetical (Ascending)",
        "Alphabetical (Descending)",
        "Number of clicks",
        "Number of logins"
      ],
      defaultDescription:
        "No Description. Sirloin short loin tenderloin tri-tip jowl chicken shank ribeye landjaeger, pancetta pork chop. Cupim filet mignon tail porchetta, biltong leberkas turkey flank pork chop frankfurter kevin short loin tenderloin tri-tip shankle. Porchetta boudin shoulder sausage, beef ribs pancetta burgdoggen prosciutto tongue. Sausage kevin strip steak, pork belly pig filet mignon chuck shankle andouille tri-tip ham cow. Pork loin t-bone doner, kevin jowl cupim sausage meatloaf.",
      launchLoading: false,
      maintenance: false,
      error: ""
    };
  },
  methods: {
    launch(appId, app) {
      this.error = "";

      this.updateClickCount(app);

      signLaunch(appId)
        .then(launchData => {
          submitLaunch(launchData)
            .then(launchResponse => {
              this.launchLoading = false;
              window.location.href = launchResponse.redirectURL;
            })
            .catch(err => {
              let code = err.response.status;
              this.launchLoading = false;
              switch (code) {
                case 500:
                  this.error =
                    "An unexpected server error occurred. Please try again momentarily.";
                  break;
                default:
                  this.error =
                    "An unexpected server error occurred. Please try again momentarily.";
                  break;
              }
            })
            .finally(() => {
              if (this.launchLoading !== false) {
                this.launchLoading = false;
                this.error =
                  "Communication with the application can not be established.";
              }
            });
        })
        .catch(err => {
          let code = err.response.status;
          this.launchLoading = false;
          switch (code) {
            case 500:
              this.error =
                "An unexpected server error occurred. Please try again momentarily.";
              break;
            default:
              this.error =
                "An unexpected server error occurred. Please try again momentarily.";
              break;
          }
        });
    },
    filterApps: function(value) {
      if (value === this.sortBy[0]) this.sortByAscending();
      else if (value === this.sortBy[1]) this.sortByDescending();
      else if (value === this.sortBy[2]) this.sortByNumberOfClicks();
      else alert("Coming Soon!");
    },
    async sortByAscending() {
      await axios
        .get(`${apiURL}/applications/ascending`)
        .then(response => (this.applications = response.data));
    },
    async sortByDescending() {
      await axios
        .get(`${apiURL}/applications/descending`)
        .then(response => (this.applications = response.data));
    },
    async sortByNumberOfClicks() {
      await axios
        .get(`${apiURL}/applications/clicks`)
        .then(response => (this.applications = response.data));
    },
    async updateClickCount(app) {
      app.ClickCount += 1;
      await axios.post(`${apiURL}/applications/update`, {
        Title: app.Title,
        Email: app.Email,
        Description: app.Description,
        LogoUrl: app.LogoUrl,
        UnderMaintenance: app.UnderMaintenance,
        ClickCount: app.ClickCount,
        headers: {
          Accept: "application/json",
          "Content-Type": "application/json"
        }
      });
    }
  },
  async mounted() {
    await axios
      .get(`${apiURL}/applications`)
      .then(response => (this.applications = response.data));
  }
};
</script>

<style scoped>
.v-card {
  margin: 1em;
}

#appPortal {
  padding: 1em 1em 0 1em;
  font-size: 38px;
  text-decoration: underline;
}

.content {
  margin-left: 1em;
  margin-right: 1em;
}

#appFilters {
  padding: 2em 3em 0 2em;
}

#maintenance {
  margin-left: 1em;
}

#launchable:hover {
  text-decoration: underline;
}

.transparent {
  background-color: white !important;
  opacity: 0.65;
  border-color: transparent !important;
}

.truncate {
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
</style>
