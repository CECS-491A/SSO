<template>
  <v-dialog v-model="dialog" width="500">

    <!-- Button to allow user to view application description -->
    <template v-slot:activator="{ on }">
      <v-chip v-on="on" class="moreInfo" color="primary" text-color="white">{{ openButton }}</v-chip>
    </template>

    <v-card>
      <!-- Displays the application title -->
      <v-card-title class="headline grey lighten-2" primary-title>{{ title }}</v-card-title>

      <!-- Displays the app description if there is one -->
      <v-card-text v-if="description !== null">{{ description }}</v-card-text>
      <v-card-text v-else>{{ defaultDescription }}</v-card-text>

      <v-card-actions>
        <v-spacer></v-spacer>
        <!-- Button to close the application information -->
        <v-btn color="primary" flat @click="dialog = false">{{ closeButton }}</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script>
export default {
  props: ["title", "description"],
  data() {
    return {
      dialog: false,
      openButton: "Description",
      closeButton: "Done",
      defaultDescription: "No description"
    };
  }
};
</script>
