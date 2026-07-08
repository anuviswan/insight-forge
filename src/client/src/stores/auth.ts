import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import { api, type UserProfile, type SignInResponse } from '../api';

export const useAuthStore = defineStore('auth', () => {
  const user = ref<UserProfile | null>(null);
  const token = ref<string | null>(null);
  const refreshToken = ref<string | null>(null);
  const loading = ref(false);
  const error = ref<string | null>(null);

  const isAuthenticated = computed(() => !!token.value);

  // Note: We do NOT restore from localStorage for mock sessions.
  // This ensures users can always access login/register pages.
  // In production with real auth, you would restore here.

  function applyTheme(isDark: boolean) {
    if (isDark) {
      document.documentElement.classList.add('dark');
    } else {
      document.documentElement.classList.remove('dark');
    }
  }

  async function signIn(email: string, password: string, rememberMe: boolean = false) {
    loading.value = true;
    error.value = null;
    try {
      const res: SignInResponse = await api.auth.signIn(email, password, rememberMe);

      if (!res.success) {
        error.value = res.message || 'Sign-in failed';
        throw new Error(res.message);
      }

      // Store tokens
      token.value = res.accessToken || null;
      if (res.refreshToken) {
        refreshToken.value = res.refreshToken;
        localStorage.setItem('refreshToken', res.refreshToken);
      }

      if (res.accessToken) {
        localStorage.setItem('token', res.accessToken);
      }

      // For now, create a basic user profile from the response
      // In production, this would be fetched from an endpoint
      if (user.value) {
        user.value.email = email;
      } else {
        user.value = {
          name: email.split('@')[0],
          email: email,
          avatar: '',
          bio: '',
          isPro: false,
          roles: [],
          preferences: {
            darkMode: false,
            autoSave: true,
            notifications: true
          },
          security: {
            twoFactor: false
          }
        };
      }

      if (user.value) {
        localStorage.setItem('user', JSON.stringify(user.value));
        applyTheme(user.value.preferences.darkMode);
      }
    } catch (err: any) {
      error.value = err.message || 'Sign-in failed';
      throw err;
    } finally {
      loading.value = false;
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
      refreshToken.value = null;
      user.value = null;
      localStorage.removeItem('token');
      localStorage.removeItem('refreshToken');
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
    refreshToken,
    loading,
    error,
    isAuthenticated,
    signIn,
    login,
    logout,
    toggleDarkMode,
    updatePreferences,
    updateSecurity,
    updateProfile
  };
});
