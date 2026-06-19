import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import { api, type UserProfile } from '../api';

export const useAuthStore = defineStore('auth', () => {
  const user = ref<UserProfile | null>(null);
  const token = ref<string | null>(null);
  const loading = ref(false);
  const error = ref<string | null>(null);

  const isAuthenticated = computed(() => !!token.value);

  // Initialize from localStorage if available (to persist mock sessions)
  const savedToken = localStorage.getItem('token');
  const savedUser = localStorage.getItem('user');
  if (savedToken && savedUser) {
    token.value = savedToken;
    try {
      user.value = JSON.parse(savedUser);
      // Sync theme on load
      applyTheme(user.value?.preferences.darkMode ?? false);
    } catch {
      localStorage.removeItem('token');
      localStorage.removeItem('user');
    }
  }

  function applyTheme(isDark: boolean) {
    if (isDark) {
      document.documentElement.classList.add('dark');
    } else {
      document.documentElement.classList.remove('dark');
    }
  }

  async function login(email: string, password?: string, provider?: 'google' | 'github') {
    loading.value = true;
    error.value = null;
    try {
      const res = await api.auth.login(email, password, provider);
      token.value = res.token;
      user.value = res.user;
      localStorage.setItem('token', res.token);
      localStorage.setItem('user', JSON.stringify(res.user));
      applyTheme(res.user.preferences.darkMode);
    } catch (err: any) {
      error.value = err.message || 'Login failed';
      throw err;
    } finally {
      loading.value = false;
    }
  }

  async function logout() {
    loading.value = true;
    try {
      await api.auth.logout();
      token.value = null;
      user.value = null;
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      document.documentElement.classList.remove('dark');
    } finally {
      loading.value = false;
    }
  }

  function toggleDarkMode() {
    if (user.value) {
      user.value.preferences.darkMode = !user.value.preferences.darkMode;
      localStorage.setItem('user', JSON.stringify(user.value));
      applyTheme(user.value.preferences.darkMode);
    }
  }

  function updatePreferences(prefs: Partial<UserProfile['preferences']>) {
    if (user.value) {
      user.value.preferences = { ...user.value.preferences, ...prefs };
      localStorage.setItem('user', JSON.stringify(user.value));
      if (prefs.darkMode !== undefined) {
        applyTheme(prefs.darkMode);
      }
    }
  }

  function updateSecurity(sec: Partial<UserProfile['security']>) {
    if (user.value) {
      user.value.security = { ...user.value.security, ...sec };
      localStorage.setItem('user', JSON.stringify(user.value));
    }
  }

  function updateProfile(name: string, bio: string, email: string) {
    if (user.value) {
      user.value.name = name;
      user.value.bio = bio;
      user.value.email = email;
      localStorage.setItem('user', JSON.stringify(user.value));
    }
  }

  return {
    user,
    token,
    loading,
    error,
    isAuthenticated,
    login,
    logout,
    toggleDarkMode,
    updatePreferences,
    updateSecurity,
    updateProfile
  };
});
