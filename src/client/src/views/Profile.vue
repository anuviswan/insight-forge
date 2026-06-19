<script setup lang="ts">
import { ref, watch } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '../stores/auth';

const authStore = useAuthStore();
const router = useRouter();

// Form bindings local values
const fullName = ref(authStore.user?.name || 'Alex Rivers');
const email = ref(authStore.user?.email || 'alex.rivers@markdownstudio.io');
const bio = ref(authStore.user?.bio || '');

const autoSave = ref(authStore.user?.preferences.autoSave ?? true);
const notifications = ref(authStore.user?.preferences.notifications ?? true);

const showSaveMessage = ref(false);

// Watch dark mode store value to sync local toggle
const darkModeLocal = ref(authStore.user?.preferences.darkMode ?? false);
watch(() => authStore.user?.preferences.darkMode, (newVal) => {
  darkModeLocal.value = newVal ?? false;
});

function handleToggleDarkMode() {
  authStore.toggleDarkMode();
}

function handleSaveChanges() {
  authStore.updateProfile(fullName.value, bio.value, email.value);
  authStore.updatePreferences({
    autoSave: autoSave.value,
    notifications: notifications.value
  });
  showSaveMessage.value = true;
  setTimeout(() => {
    showSaveMessage.value = false;
  }, 2000);
}

function handleLogout() {
  authStore.logout();
  router.push({ name: 'Login' });
}

function handleFileChange() {
  alert('Avatar upload feature simulation. Choose a premium photo or avatar.');
}
</script>

<template>
  <div class="profile-container animate-fade-in">
    <!-- Save Success Toast -->
    <div v-if="showSaveMessage" class="save-toast animate-fade-in">
      Settings saved successfully!
    </div>

    <!-- Top Identity Card -->
    <div class="identity-card card">
      <div class="identity-flex">
        <div class="avatar-uploader">
          <img :src="authStore.user?.avatar" alt="User profile avatar" class="profile-avatar-img" />
          <label class="camera-badge" for="avatar-input" aria-label="Upload profile image">
            <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
              <path d="M23 19a2 2 0 0 1-2 2H3a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h4l2-3h6l2 3h4a2 2 0 0 1 2 2z"></path>
              <circle cx="12" cy="13" r="4"></circle>
            </svg>
          </label>
          <input type="file" id="avatar-input" class="hidden-input" @change="handleFileChange" accept="image/*" />
        </div>

        <div class="identity-details">
          <div class="name-badge-row">
            <h2 class="profile-name">{{ authStore.user?.name }}</h2>
            <span class="pro-badge">PRO ACCOUNT</span>
          </div>
          <p class="profile-email text-muted">{{ authStore.user?.email }}</p>
          <div class="roles-row">
            <span v-for="role in authStore.user?.roles" :key="role" class="role-pill">{{ role }}</span>
          </div>
        </div>
      </div>
    </div>

    <!-- Sub dashboard grid -->
    <div class="profile-dashboard-grid">
      <!-- Left Column: Forms and Stats -->
      <div class="profile-left-column">
        <!-- Account Settings Card -->
        <div class="card form-card">
          <h3 class="section-title">Account Settings</h3>
          <div class="form-divider"></div>
          
          <div class="settings-row-grid">
            <div class="form-group">
              <label class="form-label" for="full-name-input">Full Name</label>
              <input type="text" id="full-name-input" v-model="fullName" class="form-control" />
            </div>
            <div class="form-group">
              <label class="form-label" for="email-address-input">Email Address</label>
              <input type="email" id="email-address-input" v-model="email" class="form-control" />
            </div>
          </div>

          <div class="form-group">
            <label class="form-label" for="bio-textarea">Bio</label>
            <textarea id="bio-textarea" rows="3" v-model="bio" class="form-control bio-textarea"></textarea>
          </div>
        </div>

        <!-- Analytics Stats Row -->
        <div class="stats-row-grid">
          <!-- Projects Usage -->
          <div class="stat-card card">
            <div class="stat-header">
              <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="stat-icon">
                <path d="M22 19a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h5l2 3h9a2 2 0 0 1 2 2z"></path>
              </svg>
              <span class="stat-title">PROJECTS</span>
            </div>
            <div class="stat-main">
              <span class="stat-big-number">4/10</span>
              <span class="stat-sub-text">40% used</span>
            </div>
            <div class="progress-bar-container">
              <div class="progress-bar-fill" style="width: 40%"></div>
            </div>
            <span class="stat-footer text-muted">Next reset in 12 days</span>
          </div>

          <!-- Words Generated -->
          <div class="stat-card card">
            <div class="stat-header">
              <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="stat-icon">
                <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path>
                <polyline points="14 2 14 8 20 8"></polyline>
                <line x1="16" y1="13" x2="8" y2="13"></line>
                <line x1="16" y1="17" x2="8" y2="17"></line>
                <polyline points="10 9 9 9 8 9"></polyline>
              </svg>
              <span class="stat-title">WORDS</span>
            </div>
            <div class="stat-main">
              <span class="stat-big-number">12,400</span>
              <span class="stat-sub-text">This month</span>
            </div>
            <div class="progress-bar-container">
              <div class="progress-bar-fill accent" style="width: 65%"></div>
            </div>
            <span class="stat-footer text-muted">2.4k generated today</span>
          </div>
        </div>
      </div>

      <!-- Right Column: Preferences and Controls -->
      <div class="profile-right-column">
        <!-- Preferences -->
        <div class="card pref-card">
          <h3 class="section-title">Preferences</h3>
          <div class="form-divider"></div>

          <div class="toggle-control-item">
            <div class="toggle-labels">
              <span class="toggle-name">Dark Mode</span>
              <span class="toggle-sub text-muted">Use dark theme at night</span>
            </div>
            <label class="switch-toggle" for="dark-mode-toggle">
              <input type="checkbox" id="dark-mode-toggle" v-model="darkModeLocal" @change="handleToggleDarkMode" />
              <span class="switch-slider"></span>
            </label>
          </div>

          <div class="toggle-control-item">
            <div class="toggle-labels">
              <span class="toggle-name">Auto-save</span>
              <span class="toggle-sub text-muted">Save Markdown every 30s</span>
            </div>
            <label class="switch-toggle" for="autosave-toggle">
              <input type="checkbox" id="autosave-toggle" v-model="autoSave" />
              <span class="switch-slider"></span>
            </label>
          </div>

          <div class="toggle-control-item">
            <div class="toggle-labels">
              <span class="toggle-name">Notifications</span>
              <span class="toggle-sub text-muted">Email updates & alerts</span>
            </div>
            <label class="switch-toggle" for="notifications-toggle">
              <input type="checkbox" id="notifications-toggle" v-model="notifications" />
              <span class="switch-slider"></span>
            </label>
          </div>
        </div>

        <!-- Security -->
        <div class="card pref-card">
          <h3 class="section-title">Security</h3>
          <div class="form-divider"></div>

          <button class="security-list-btn">
            <div class="btn-left-content">
              <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <rect x="3" y="11" width="18" height="11" rx="2" ry="2"></rect>
                <path d="M7 11V7a5 5 0 0 1 10 0v4"></path>
              </svg>
              <span>Change Password</span>
            </div>
            <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
              <polyline points="9 18 15 12 9 6"></polyline>
            </svg>
          </button>

          <div class="security-row">
            <div class="btn-left-content">
              <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"></path>
              </svg>
              <span>Two-Factor Auth</span>
            </div>
            <span class="security-badge off">OFF</span>
          </div>
        </div>

        <!-- Save and Logout Stack -->
        <div class="action-buttons-stack">
          <button class="btn btn-primary main-save-btn" @click="handleSaveChanges">Save Changes</button>
          <button class="btn btn-secondary logout-btn" @click="handleLogout">
            <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"></path>
              <polyline points="16 17 21 12 16 7"></polyline>
              <line x1="21" y1="12" x2="9" y2="12"></line>
            </svg>
            Log Out
          </button>
        </div>
      </div>
    </div>

    <!-- Global Hotkey Zen mode info -->
    <div class="zen-mode-info">
      PRESS <span class="key-indicator">Z</span> TO ENTER ZEN MODE
    </div>
  </div>
</template>

<style scoped>
.profile-container {
  max-width: 900px;
  margin: 0 auto;
  width: 100%;
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
  padding-bottom: var(--space-xxl);
}

.save-toast {
  position: fixed;
  top: 20px;
  right: 20px;
  background-color: #10b981;
  color: #ffffff;
  padding: 12px var(--space-xl);
  border-radius: var(--radius-md);
  font-weight: 600;
  box-shadow: var(--shadow-lg);
  z-index: 1000;
}

.identity-card {
  padding: var(--space-lg) var(--space-xl);
}

.identity-flex {
  display: flex;
  align-items: center;
  gap: var(--space-xl);
}

.avatar-uploader {
  position: relative;
  width: 90px;
  height: 90px;
}

.profile-avatar-img {
  width: 100%;
  height: 100%;
  border-radius: var(--radius-md);
  object-fit: cover;
  border: 1px solid var(--border-color);
}

.camera-badge {
  position: absolute;
  bottom: -6px;
  right: -6px;
  width: 28px;
  height: 28px;
  background-color: var(--primary-color);
  color: #ffffff;
  border-radius: var(--radius-sm);
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  box-shadow: var(--shadow-primary);
  border: 2px solid var(--bg-card);
  transition: background-color var(--transition-fast);
}

.camera-badge:hover {
  background-color: var(--primary-hover);
}

.hidden-input {
  display: none;
}

.identity-details {
  display: flex;
  flex-direction: column;
  gap: var(--space-xs);
}

.name-badge-row {
  display: flex;
  align-items: center;
  gap: var(--space-md);
}

.profile-name {
  font-size: var(--font-size-xl);
  font-weight: 700;
}

.pro-badge {
  font-size: 10px;
  background-color: #d1fae5;
  color: #065f46;
  padding: 2px var(--space-sm);
  border-radius: var(--radius-full);
  font-weight: 700;
  letter-spacing: 0.05em;
}

.dark .pro-badge {
  background-color: rgba(6, 95, 70, 0.2);
  color: #34d399;
}

.profile-email {
  font-size: var(--font-size-sm);
}

.roles-row {
  display: flex;
  gap: var(--space-xs);
  margin-top: 4px;
}

.role-pill {
  font-size: 10px;
  background-color: var(--primary-light);
  color: var(--primary-color);
  padding: 2px 8px;
  border-radius: var(--radius-sm);
  font-weight: 700;
  letter-spacing: 0.02em;
}

.profile-dashboard-grid {
  display: grid;
  grid-template-columns: 1.2fr 1fr;
  gap: var(--space-lg);
}

.profile-left-column {
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
}

.profile-right-column {
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
}

.section-title {
  font-size: var(--font-size-base);
  font-weight: 700;
  color: var(--text-primary);
}

.form-divider {
  height: 1px;
  background-color: var(--border-color);
  margin: var(--space-md) 0;
}

.settings-row-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: var(--space-md);
}

.bio-textarea {
  resize: vertical;
}

.stats-row-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: var(--space-md);
}

.stat-card {
  padding: var(--space-md) var(--space-lg);
}

.stat-header {
  display: flex;
  align-items: center;
  gap: var(--space-xs);
  color: var(--text-secondary);
}

.stat-icon {
  color: var(--primary-color);
}

.stat-title {
  font-size: 11px;
  font-weight: 700;
  letter-spacing: 0.05em;
}

.stat-main {
  display: flex;
  align-items: baseline;
  justify-content: space-between;
  margin-top: var(--space-sm);
  margin-bottom: var(--space-sm);
}

.stat-big-number {
  font-size: var(--font-size-xl);
  font-weight: 700;
  color: var(--text-primary);
}

.stat-sub-text {
  font-size: var(--font-size-xs);
  font-weight: 600;
  color: var(--text-muted);
}

.progress-bar-container {
  height: 6px;
  background-color: var(--border-color);
  border-radius: var(--radius-full);
  overflow: hidden;
  margin-bottom: var(--space-sm);
}

.progress-bar-fill {
  height: 100%;
  background-color: var(--primary-color);
  border-radius: var(--radius-full);
}

.progress-bar-fill.accent {
  background-color: #6366f1;
}

.stat-footer {
  font-size: 11px;
  font-weight: 500;
}

.toggle-control-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: var(--space-sm) 0;
}

.toggle-labels {
  display: flex;
  flex-direction: column;
}

.toggle-name {
  font-size: var(--font-size-sm);
  font-weight: 600;
  color: var(--text-primary);
}

.toggle-sub {
  font-size: var(--font-size-xs);
}

/* Switch styling */
.switch-toggle {
  position: relative;
  display: inline-block;
  width: 44px;
  height: 24px;
}

.switch-toggle input {
  opacity: 0;
  width: 0;
  height: 0;
}

.switch-slider {
  position: absolute;
  cursor: pointer;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: var(--border-color);
  transition: .3s;
  border-radius: var(--radius-full);
}

.switch-slider:before {
  position: absolute;
  content: "";
  height: 16px;
  width: 16px;
  left: 4px;
  bottom: 4px;
  background-color: #ffffff;
  transition: .3s;
  border-radius: 50%;
}

input:checked + .switch-slider {
  background-color: var(--primary-color);
}

input:checked + .switch-slider:before {
  transform: translateX(20px);
}

.security-list-btn {
  width: 100%;
  display: flex;
  align-items: center;
  justify-content: space-between;
  background-color: var(--bg-card);
  border: 1px solid var(--border-color);
  padding: 10px var(--space-md);
  border-radius: var(--radius-md);
  cursor: pointer;
  color: var(--text-secondary);
  font-weight: 600;
  font-size: var(--font-size-sm);
  transition: all var(--transition-fast);
  margin-bottom: var(--space-sm);
}

.security-list-btn:hover {
  background-color: var(--bg-input);
  color: var(--text-primary);
}

.btn-left-content {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
}

.security-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 10px var(--space-md);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  color: var(--text-secondary);
  font-weight: 600;
  font-size: var(--font-size-sm);
}

.security-badge {
  font-size: 10px;
  padding: 2px 6px;
  border-radius: var(--radius-sm);
  font-weight: 700;
}

.security-badge.off {
  background-color: rgba(225, 29, 72, 0.08);
  color: #e11d48;
  border: 1px solid rgba(225, 29, 72, 0.15);
}

.action-buttons-stack {
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  margin-top: var(--space-sm);
}

.main-save-btn {
  padding: 12px;
  font-size: var(--font-size-base);
}

.logout-btn {
  padding: 12px;
  font-size: var(--font-size-base);
  border-color: rgba(225, 29, 72, 0.4);
  color: #e11d48;
}
.logout-btn:hover {
  background-color: rgba(225, 29, 72, 0.05);
  border-color: #e11d48;
}

.zen-mode-info {
  text-align: center;
  font-size: 11px;
  font-weight: 600;
  color: var(--text-muted);
  letter-spacing: 0.1em;
  margin-top: var(--space-lg);
}

.key-indicator {
  border: 1px solid var(--border-color);
  background-color: var(--bg-card);
  padding: 2px 6px;
  border-radius: 4px;
  font-family: var(--font-mono);
  box-shadow: var(--shadow-sm);
}
</style>
