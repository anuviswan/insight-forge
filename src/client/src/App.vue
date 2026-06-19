<script setup lang="ts">
import { onMounted } from 'vue';
import { useRoute } from 'vue-router';
import { useAuthStore } from './stores/auth';
import Sidebar from './components/Sidebar.vue';
import Header from './components/Header.vue';

const authStore = useAuthStore();
const route = useRoute();

onMounted(() => {
  // Sync initial dark mode state from store preferences if user exists
  if (authStore.user?.preferences.darkMode) {
    document.documentElement.classList.add('dark');
  } else {
    document.documentElement.classList.remove('dark');
  }
});
</script>

<template>
  <div class="app-container">
    <!-- Render sidebar if user is logged in, or background style if logging in -->
    <Sidebar v-if="authStore.isAuthenticated || route.name === 'Login'" :class="{ 'inert-background': route.name === 'Login' }" />
    
    <div class="main-content">
      <Header v-if="authStore.isAuthenticated || route.name === 'Login'" :class="{ 'inert-background': route.name === 'Login' }" />
      
      <main class="page-body">
        <router-view />
      </main>
    </div>
  </div>
</template>

<style>
.inert-background {
  opacity: 0.45;
  pointer-events: none;
  filter: blur(0.5px);
  user-select: none;
}
</style>
