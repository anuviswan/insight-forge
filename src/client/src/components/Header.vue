<script setup lang="ts">
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '../stores/auth';
import { useDocumentsStore } from '../stores/documents';

const authStore = useAuthStore();
const documentsStore = useDocumentsStore();
const router = useRouter();

const showDropdown = ref(false);

function handleToggleDarkMode() {
  authStore.toggleDarkMode();
}

function handleLogout() {
  authStore.logout();
  router.push({ name: 'Login' });
}

function handleProfileClick() {
  showDropdown.value = false;
  router.push({ name: 'Profile' });
}
</script>

<template>
  <header class="app-header">
    <div class="header-left">
      <div class="search-container">
        <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" class="search-icon">
          <circle cx="11" cy="11" r="8"></circle>
          <line x1="21" y1="21" x2="16.65" y2="16.65"></line>
        </svg>
        <input 
          type="text" 
          placeholder="Search documents..." 
          v-model="documentsStore.searchHistoryQuery"
          class="search-input"
        />
      </div>
    </div>
    
    <div class="header-right">
      <!-- Zen mode indicator -->
      <div class="zen-indicator">
        <span class="zen-text">ZEN_</span>
        <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="zen-icon">
          <path d="M12 20h9"></path>
          <path d="M16.5 3.5a2.121 2.121 0 0 1 3 3L7 19l-4 1 1-4L16.5 3.5z"></path>
        </svg>
      </div>

      <!-- Theme Switcher -->
      <button class="header-btn" @click="handleToggleDarkMode" aria-label="Toggle Dark Mode">
        <svg v-if="authStore.user?.preferences.darkMode" xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <circle cx="12" cy="12" r="5"></circle>
          <line x1="12" y1="1" x2="12" y2="3"></line>
          <line x1="12" y1="21" x2="12" y2="23"></line>
          <line x1="4.22" y1="4.22" x2="5.64" y2="5.64"></line>
          <line x1="18.36" y1="18.36" x2="19.78" y2="19.78"></line>
          <line x1="1" y1="12" x2="3" y2="12"></line>
          <line x1="21" y1="12" x2="23" y2="12"></line>
          <line x1="4.22" y1="19.78" x2="5.64" y2="18.36"></line>
          <line x1="18.36" y1="5.64" x2="19.78" y2="4.22"></line>
        </svg>
        <svg v-else xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <path d="M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79z"></path>
        </svg>
      </button>

      <!-- Settings Icon -->
      <button class="header-btn" @click="router.push({ name: 'Profile' })" aria-label="Settings">
        <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <circle cx="12" cy="12" r="3"></circle>
          <path d="M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 1 1-2.83 2.83l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 0 1-4 0v-.09A1.65 1.65 0 0 0 9 19.4a1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 1 1-2.83-2.83l.06-.06a1.65 1.65 0 0 0 .33-1.82 1.65 1.65 0 0 0-1.51-1H3a2 2 0 0 1 0-4h.09A1.65 1.65 0 0 0 4.6 9a1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 1 1 2.83-2.83l.06.06a1.65 1.65 0 0 0 1.82.33H9a1.65 1.65 0 0 0 1-1.51V3a2 2 0 0 1 4 0v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 1 1 2.83 2.83l-.06.06a1.65 1.65 0 0 0-.33 1.82V9a1.65 1.65 0 0 0 1.51 1H21a2 2 0 0 1 0 4h-.09a1.65 1.65 0 0 0-1.51 1z"></path>
        </svg>
      </button>

      <!-- Profile Dropdown -->
      <div class="profile-menu-container">
        <button class="avatar-btn" @click="showDropdown = !showDropdown" aria-label="User Profile Dropdown">
          <img :src="authStore.user?.avatar" alt="Avatar" class="avatar-img" v-if="authStore.user?.avatar" />
          <div class="avatar-fallback" v-else>
            {{ authStore.user?.name.charAt(0) || 'U' }}
          </div>
        </button>

        <Transition name="fade-slide">
          <div class="profile-dropdown" v-if="showDropdown">
            <div class="dropdown-header">
              <span class="user-name">{{ authStore.user?.name }}</span>
              <span class="user-email">{{ authStore.user?.email }}</span>
            </div>
            <div class="dropdown-divider"></div>
            <button class="dropdown-item" @click="handleProfileClick">
              <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"></path>
                <circle cx="12" cy="7" r="4"></circle>
              </svg>
              My Profile
            </button>
            <button class="dropdown-item" @click="router.push({ name: 'Blogger' }); showDropdown = false;">
              <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <path d="M12 20h9"></path>
                <path d="M16.5 3.5a2.121 2.121 0 0 1 3 3L7 19l-4 1 1-4L16.5 3.5z"></path>
              </svg>
              Blogger Mode
            </button>
            <button class="dropdown-item" @click="router.push({ name: 'Summariser' }); showDropdown = false;">
              <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path>
                <polyline points="14 2 14 8 20 8"></polyline>
                <line x1="16" y1="13" x2="8" y2="13"></line>
                <line x1="16" y1="17" x2="8" y2="17"></line>
                <polyline points="10 9 9 9 8 9"></polyline>
              </svg>
              Summariser
            </button>
            <div class="dropdown-divider"></div>
            <button class="dropdown-item text-danger" @click="handleLogout">
              <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"></path>
                <polyline points="16 17 21 12 16 7"></polyline>
                <line x1="21" y1="12" x2="9" y2="12"></line>
              </svg>
              Log Out
            </button>
          </div>
        </Transition>
      </div>
    </div>
  </header>
</template>

<style scoped>
.app-header {
  height: 70px;
  background-color: var(--bg-card);
  border-bottom: 1px solid var(--border-color);
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 var(--space-xl);
  z-index: 10;
  transition: background-color var(--transition-normal), border-color var(--transition-normal);
}

.header-left {
  display: flex;
  align-items: center;
}

.search-container {
  position: relative;
  width: 320px;
}

.search-icon {
  position: absolute;
  left: 14px;
  top: 50%;
  transform: translateY(-50%);
  color: var(--text-muted);
  pointer-events: none;
}

.search-input {
  width: 100%;
  padding: 10px 14px 10px 42px;
  border-radius: var(--radius-md);
  border: 1px solid var(--border-color);
  background-color: var(--bg-input);
  color: var(--text-primary);
  font-size: var(--font-size-sm);
  transition: all var(--transition-fast);
}

.search-input:focus {
  outline: none;
  border-color: var(--border-focus);
  background-color: var(--bg-card);
  box-shadow: 0 0 0 3px rgba(62, 76, 211, 0.12);
}

.header-right {
  display: flex;
  align-items: center;
  gap: var(--space-md);
}

.zen-indicator {
  display: flex;
  align-items: center;
  gap: var(--space-xs);
  background-color: var(--bg-input);
  padding: 6px 12px;
  border-radius: var(--radius-sm);
  font-weight: 700;
  font-size: var(--font-size-sm);
  letter-spacing: 0.05em;
  color: var(--text-secondary);
}

.zen-text {
  font-family: var(--font-mono);
}

.zen-icon {
  color: var(--text-muted);
}

.header-btn {
  background: none;
  border: none;
  color: var(--text-secondary);
  cursor: pointer;
  width: 40px;
  height: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: var(--radius-md);
  transition: all var(--transition-fast);
}

.header-btn:hover {
  background-color: var(--bg-input);
  color: var(--text-primary);
}

.profile-menu-container {
  position: relative;
}

.avatar-btn {
  background: none;
  border: none;
  cursor: pointer;
  width: 40px;
  height: 40px;
  border-radius: var(--radius-full);
  overflow: hidden;
  display: flex;
  align-items: center;
  justify-content: center;
  border: 2px solid var(--border-color);
  transition: border-color var(--transition-fast);
}

.avatar-btn:hover {
  border-color: var(--primary-color);
}

.avatar-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.avatar-fallback {
  width: 100%;
  height: 100%;
  background-color: var(--primary-color);
  color: #ffffff;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 600;
}

.profile-dropdown {
  position: absolute;
  right: 0;
  top: 52px;
  width: 240px;
  background-color: var(--bg-card);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  box-shadow: var(--shadow-lg);
  padding: var(--space-sm);
  display: flex;
  flex-direction: column;
}

.dropdown-header {
  padding: var(--space-sm) var(--space-md);
  display: flex;
  flex-direction: column;
}

.user-name {
  font-weight: 600;
  font-size: var(--font-size-base);
  color: var(--text-primary);
}

.user-email {
  font-size: var(--font-size-xs);
  color: var(--text-muted);
}

.dropdown-divider {
  height: 1px;
  background-color: var(--border-color);
  margin: var(--space-sm) 0;
}

.dropdown-item {
  background: none;
  border: none;
  text-align: left;
  padding: 10px var(--space-md);
  font-size: var(--font-size-sm);
  color: var(--text-secondary);
  border-radius: var(--radius-sm);
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  transition: all var(--transition-fast);
}

.dropdown-item:hover {
  background-color: var(--bg-input);
  color: var(--text-primary);
}

.dropdown-item.text-danger {
  color: #e11d48;
}
.dropdown-item.text-danger:hover {
  background-color: rgba(225, 29, 72, 0.08);
}

.fade-slide-enter-active,
.fade-slide-leave-active {
  transition: all 0.2s ease-out;
}

.fade-slide-enter-from,
.fade-slide-leave-to {
  opacity: 0;
  transform: translateY(-8px);
}
</style>
