<template>
  <div>
    <v-card xs12>
      <v-layout row wrap>

        <!-- Title of the page -->
        <v-flex lg9>
          <h1 id="appPortal">{{ pageTitle }}</h1>
        </v-flex>
        <!-- Sorts the applications -->
        <v-flex lg3 id="sortApps">
          <v-select @change="getApplications()" v-model="pagination.sortOrder" :items="text.sortByItems" :label="text.sortByLabel"></v-select>
        </v-flex>
      </v-layout>

      <v-container fluid grid-list-md>
        <v-layout row wrap>
          <v-flex xs12 md6 lg6 v-for="app in applications" :key="app.Id">

            <!-- Loading the apps gives time to perform initial health check -->
            <div class="text-xs-center" v-if="appLoading">
              <v-progress-circular :size="100" color="primary" indeterminate></v-progress-circular>
            </div>
            <!-- After health check, show apps -->
            <div v-else>
              <!-- The card that shows up if the app IS under maintenance -->
              <v-card v-if="(app.UnderMaintenance || healthCheck.HealthStatuses[app.Id])" class="transparent">
                <v-card-title primary-title>
                  <!-- If there is no logo, then a default image will be shown -->
                  <img v-if="app.LogoUrl === null" src="@/assets/no-image-icon.png">
                  <img v-else :src="app.LogoUrl">

                  <div class="content">
                    <!-- Launching to an app can be done by clicking the app title -->
                    <h3 class="headline mb-0">
                      <strong class="truncate">{{ app.Title | truncate(maxTitleLength, text.textTail)}}</strong>
                    </h3>
                  </div>
                </v-card-title>

                <div class="appDesc">
                  <!-- Shows button that displays app description -->
                  <AppDetails v-if="app.Description != ''" :title="app.Title" :description="app.Description" />
                  <AppDetails v-else :title="app.Title" :description="defaultDescription" />

                  <!-- Shows button that displays popularity of app -->
                  <v-chip color="indigo" text-color="white">Popularity: {{ app.ClickCount | truncate(text.maxPopularityLength, text.textTail)}}</v-chip>

                  <!-- Shows button that displays that an app is under maintenance -->
                  <v-chip color="orange" text-color="white">
                    Under Maintenance
                    <v-icon right large>build</v-icon>
                  </v-chip>
                </div>
              </v-card>

              <!-- The card that shows up if the app IS NOT under maintenance -->
              <v-card v-else hover>
                <v-card-title primary-title>
                  <!-- If there is no logo, then a default image will be shown -->
                  <img v-if="app.LogoUrl === null" src="@/assets/no-image-icon.png" @click="launch(app.Id, app)">
                  <img v-else :src="app.LogoUrl" @click="launch(app.Id, app)">

                  <div id="content" v-if="app.UnderMaintenance || healthCheck.HealthStatuses[app.Id]">
                    <!-- Launching to an app can be done by clicking the app title -->
                    <h3 class="headline mb-0" row wrap>
                      <strong>{{ app.Title | truncate(maxTitleLength, textTail)}}</strong>
                    </h3>
                  </div>

                  <div class="content" v-else>
                    <!-- Launching to an app can be done by clicking the app title -->
                    <h3 id="launchable" class="headline mb-0" @click="launch(app.Id, app)">
                      <strong>{{ app.Title | truncate(maxTitleLength, text.textTail)}}</strong>
                    </h3>
                  </div>
                </v-card-title>

                <div class="appDesc">
                  <!-- Shows button to view description of application -->
                  <AppDetails :title="app.Title" :description="app.Description" />
                  <!-- Shows button to view popularity of application -->
                  <v-chip color="indigo" text-color="white">Popularity: {{ app.ClickCount | truncate(text.maxPopularityLength, text.textTail) }}</v-chip>
                </div>
              </v-card>
            </div>
          </v-flex>
        </v-layout>
      </v-container>
      <!-- shows loading screen only if app is in progress of launching -->
      <div v-if="launchLoading">
        <Loading :dialog="launchLoading" />
      </div>

      <!-- Shows additional functionality for the user -->
      <v-container grid-list-sm text-xs-center>
        <v-layout row wrap>
          <v-flex xs12 lg4>
          </v-flex>
          <v-flex xs12 sm6 md6 lg4>
            <div class="text-xs-center">
              <!-- Application pagination -->
              <v-pagination v-model="pagination.currentPage" :length="pagination.totalPages" :total-visible="pagination.totalVisible" @input="getApplications()"></v-pagination>
            </div>
          </v-flex>

          <v-flex xs12 sm6 md6 lg4>
            <!-- Changes the number of items to display on the page -->
            <v-select v-model="pagination.pageSize" @change="getApplications()" :items="text.displayItems" :label="text.displayLabel"></v-select>
          </v-flex>
        </v-layout>
      </v-container>
    </v-card>
      
    <!-- Loads only if app is in progress of launching -->
    <Loading :dialog="launchLoading" :text="loadingText" />
    <div v-if="popupshow">
      <PopupDialog :dialog="popupshow" :text="popuptext" :redirect="false" :route="true" :routeTo="popuprouteTo" />
    </div>
    <v-alert :value="error" type="error" transition="scale-transition">{{error}}</v-alert>
  </div>
</template>

<script>
import Vue from "vue";
import axios from "axios";
import Loading from "@/components/Dialogs/Loading.vue";
import AppDetails from "@/components/Dialogs/AppDetails.vue";
import PopupDialog from '@/components/Dialogs/PopupDialog.vue';
import { signAndLaunch } from "@/services/oauth";
import { filter } from "@/services/TextFormat";
import { apiURL } from "@/const.js";
import { store } from '@/services/request';

Vue.filter("truncate", filter);

export default {
  name: "Dashboard",
  components: { Loading, AppDetails, PopupDialog },
  data() {
    return {
      // Main attributes of the page
      pageTitle: "Application Portal",
      applications: [],
      appLoading: true,
      launchLoading: false,
      loadingText: "",
      maintenance: false,
      error: "",
      popupshow: false,
      popuptext: "",
      popuprouteTo: "/login",

      // Everything involving text goes here
      text: {
        defaultText: "Default Text",
        textTail: "...",
        maxPopularityLength: 5,
        sortByLabel: "Sort by",
        sortByItems: ["Ascending", "Descending", "Popularity", "Default"],
        displayLabel: "Items per page",
        displayItems: [2, 4, 6, 8, 100]
      },

      // Everything involving pagination goes here
      pagination: {
        currentPage: 1,
        pageSize: 8,
        sortOrder: "",
        startingIndex: 1,
        totalPages: 1,
        totalVisible: 7
      },

      // Everything involving health check goes here
      healthCheck: {
        LastHealthCheck: new Date(),
        HealthStatuses: {},
        interval: 11
      },

    }
  },
  computed: {
    maxTitleLength: function() {
      var maxTitleLength = 0;

      // If window size is greated than average mobile device,
      // set max length of app title to 25 chars. Otherwise,
      // set it to 16 chars
      if (window.innerWidth > 414) maxTitleLength = 25;
      else maxTitleLength = 16;
      return maxTitleLength;
    }
  },
  methods: {
    /**
     * This method uses the SSO API to launch the application
     * in a new tab
     *
     * @param {String} appId
     *  The id of the application originally a guid, parsed into a string
     * @param {Object} app
     *  An individual application from the list of available applications
     */
    launch(appId, app) {
      this.error = "";
      // Updates click count when launching app
      this.updateClickCount(app);
      this.launchLoading = true;
      this.loadingText= "Launching Application...";

      // Using the appId, launches the app when click
      signAndLaunch(appId)
        .catch(e=> {
          this.popuprouteTo = "/login";
          this.popuptext = e.message;
          this.popupshow = true;
          localStorage.removeItem('token');
          store.state.isLogin = false;
          this.error = e.message;
        })
        // Regardless of passing or failing, cancel launch loading animation
        .finally(() => {
          this.launchLoading = false;
        });
    },

    /**
     * This method gets applications using the SSO API.
     * It is paginated to increase performance.
     */
    async getApplications() {
      await axios
        .get(`${apiURL}/applications`, {
          params: {
            // Current page the user is viewing
            currentPage: this.pagination.currentPage,
            // Amount of items to display on the page
            pageSize: this.pagination.pageSize,
            // Order in which the apps are sorted
            sortOrder: this.pagination.sortOrder
          }
        })
        .then(response => {
          // Sets the apps to display
          this.applications = response.data.PaginatedApplications;
          // Sets total number of pages for pagination
          this.pagination.totalPages = response.data.TotalPages;
        })
        .catch(error => {
          this.error = error.message;
        });
    },

    /**
     * This method checks the health of each app so that if an app
     * is down then it is put in maintenance mode, making it unclickable.
     * The health check is performed after timing out for a few seconds.
     */
    async getApplicationHealth() {
      await axios
        .get(`${apiURL}/applications/healthcheck`)
        .then(response => {
          // Date and time of last health check in UTC
          this.healthCheck.LastHealthCheck = response.data.LastHealthCheck;
          // The health statuses of each app
          this.healthCheck.HealthStatuses = response.data.HealthStatuses;
        })
        .catch(error => {
          this.error = error.message;
        });

      // After timing out for the specified interval, perform another health check
      setTimeout(() => {
        this.getApplicationHealth();
      }, this.healthCheck.interval * 1000);
    },

    /**
     * This method updates the click count of an app, thus,
     * increasing its "popularity"
     *
     * @param {Object} app
     *  The specific app whose click count will be updated
     */
    async updateClickCount(app) {
      app.ClickCount += 1; // Increase click coutn by 1 each time
      await axios
        .post(`${apiURL}/applications/update`, {
          Title: app.Title,
          Email: app.Email,
          Description: app.Description,
          LogoUrl: app.LogoUrl,
          UnderMaintenance: app.UnderMaintenance,
          ClickCount: app.ClickCount
        })
        .then(this.getApplications())
        .catch(error => {
          this.error = error.message;
        });
    }
  },

  // Initialize reactive data properties.
  // DOM not mounted yet
  created() {
    this.getApplications();
  },
  // Perform DOM manipulation since its mounted
  async mounted() {
    await this.getApplicationHealth();

    // Set app loading to false after health check
    this.appLoading = false;
  }
};
</script>

<style lang="css" scoped>
#appPortal {
  padding: 1em 1em 0 1em;
  font-size: 38px;
  text-decoration: underline;
}

.content {
  margin-left: 1em;
  margin-right: 1em;
}

#sortApps {
  padding: 2em 3em 0 2em;
}

.appDesc {
  padding-bottom: 1em;
  margin: 0 1em;
}

#launchable:hover {
  text-decoration: underline;
}

.transparent {
  background-color: white !important;
  opacity: 0.65;
  border-color: transparent !important;
}
</style>
