<script setup lang="ts">
import { onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useAuthStore } from '../stores/auth';
import { useDocumentsStore, type HistoryItem } from '../stores/documents';

const authStore = useAuthStore();
const documentsStore = useDocumentsStore();
const route = useRoute();
const router = useRouter();

onMounted(() => {
  // Load history on mount if user is logged in
  if (authStore.isAuthenticated) {
    documentsStore.loadHistory();
  }
});

function handleNewDocument() {
  documentsStore.addEmptyDocument();
  router.push({ name: 'Blogger' });
}

function handleHistoryClick(item: HistoryItem) {
  if (item.type === 'blog') {
    // Mock loading active blog post
    documentsStore.activePost = {
      title: item.title,
      content: `# ${item.title}\n\nThis is the content loaded from history for "${item.title}". You can continue editing or downloading it.`,
      wordCount: 120,
      readTime: '1m',
      imageUrl: 'https://images.unsplash.com/photo-1618005182384-a83a8bd57fbe?auto=format&fit=crop&w=800&q=80'
    };
    router.push({ name: 'Blogger' });
  } else if (item.type === 'summary') {
    // Mock loading active summary
    documentsStore.activeSummary = {
      title: item.title,
      content: `### Executive Overview\n\nThis is the loaded summary content for "${item.title}".\n\n### Key Insights & Bullet Points\n\n* **Detail 1:** Loaded detail about quantum research.\n* **Detail 2:** Additional simulated findings.`,
      sourcesAnalyzed: 3,
      readingTimeSaved: 10,
      sizeKb: 0.9
    };
    router.push({ name: 'Summariser' });
  }
}
</script>

<template>
  <aside class="app-sidebar">
    <div class="sidebar-brand">
      <div class="logo-icon-container">
        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" class="logo-icon">
          <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path>
          <polyline points="14 2 14 8 20 8"></polyline>
          <line x1="16" y1="13" x2="8" y2="13"></line>
          <line x1="16" y1="17" x2="8" y2="17"></line>
          <polyline points="10 9 9 9 8 9"></polyline>
        </svg>
      </div>
      <div class="brand-text">
        <span class="brand-name">Markdown Studio</span>
        <span class="brand-sub">AI WRITING SUITE</span>
      </div>
    </div>

    <!-- Navigation links -->
    <nav class="sidebar-nav">
      <!-- New Document action -->
      <button class="btn btn-primary new-doc-btn" @click="handleNewDocument">
        <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
          <line x1="12" y1="5" x2="12" y2="19"></line>
          <line x1="5" y1="12" x2="19" y2="12"></line>
        </svg>
        New Document
      </button>

      <div class="nav-links">
        <router-link :to="{ name: 'Blogger' }" class="nav-link" :class="{ active: route.name === 'Blogger' }">
          <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"></path>
            <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"></path>
          </svg>
          Blogger
        </router-link>

        <router-link :to="{ name: 'Summariser' }" class="nav-link" :class="{ active: route.name === 'Summariser' }">
          <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path>
            <polyline points="14 2 14 8 20 8"></polyline>
            <line x1="16" y1="13" x2="8" y2="13"></line>
            <line x1="16" y1="17" x2="8" y2="17"></line>
            <polyline points="10 9 9 9 8 9"></polyline>
          </svg>
          Summariser
        </router-link>

        <router-link :to="{ name: 'Profile' }" class="nav-link" :class="{ active: route.name === 'Profile' }">
          <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"></path>
            <circle cx="12" cy="7" r="4"></circle>
          </svg>
          Profile
        </router-link>
      </div>
    </nav>

    <!-- Recent History Section -->
    <div class="sidebar-history-section">
      <span class="history-title">Recent History</span>
      
      <div class="history-search-container">
        <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" class="history-search-icon">
          <circle cx="11" cy="11" r="8"></circle>
          <line x1="21" y1="21" x2="16.65" y2="16.65"></line>
        </svg>
        <input 
          type="text" 
          placeholder="Search history..." 
          v-model="documentsStore.searchHistoryQuery"
          class="history-search-input"
        />
      </div>

      <div class="history-list">
        <div 
          v-for="item in documentsStore.filteredHistory" 
          :key="item.id" 
          class="history-item"
          @click="handleHistoryClick(item)"
        >
          <div class="history-item-content">
            <span class="history-item-name">{{ item.title }}</span>
            <span class="history-item-date">{{ item.date }}</span>
          </div>
          <button 
            class="history-delete-btn" 
            @click.stop="documentsStore.deleteHistoryItem(item.id)"
            aria-label="Delete history item"
          >
            <svg xmlns="http://www.w3.org/2000/svg" width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <polyline points="3 6 5 6 21 6"></polyline>
              <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path>
            </svg>
          </button>
        </div>
        <div v-if="documentsStore.filteredHistory.length === 0" class="history-empty">
          No history found
        </div>
      </div>
    </div>

    <!-- Bottom Actions -->
    <div class="sidebar-footer">
      <a href="#" class="footer-link" @click.prevent="router.push({ name: 'Profile' })">
        <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <circle cx="12" cy="12" r="10"></circle>
          <path d="M9.09 9a3 3 0 0 1 5.83 1c0 2-3 3-3 3"></path>
          <line x1="12" y1="17" x2="12.01" y2="17"></line>
        </svg>
        Help
      </a>
      <a href="#" class="footer-link" @click.prevent="router.push({ name: 'Profile' })">
        <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <circle cx="12" cy="12" r="3"></circle>
          <path d="M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 1 1-2.83 2.83l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 0 1-4 0v-.09A1.65 1.65 0 0 0 9 19.4a1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 1 1-2.83-2.83l.06-.06a1.65 1.65 0 0 0 .33-1.82 1.65 1.65 0 0 0-1.51-1H3a2 2 0 0 1 0-4h.09A1.65 1.65 0 0 0 4.6 9a1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 1 1 2.83-2.83l.06.06a1.65 1.65 0 0 0 1.82.33H9a1.65 1.65 0 0 0 1-1.51V3a2 2 0 0 1 4 0v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 1 1 2.83 2.83l-.06.06a1.65 1.65 0 0 0-.33 1.82V9a1.65 1.65 0 0 0 1.51 1H21a2 2 0 0 1 0 4h-.09a1.65 1.65 0 0 0-1.51 1z"></path>
        </svg>
        Settings
      </a>
    </div>
  </aside>
</template>

<style scoped>
.app-sidebar {
  width: 260px;
  background-color: var(--bg-sidebar);
  border-right: 1px solid var(--border-color);
  display: flex;
  flex-direction: column;
  height: 100vh;
  z-index: 11;
  transition: background-color var(--transition-normal), border-color var(--transition-normal);
}

.sidebar-brand {
  padding: var(--space-lg);
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  border-bottom: 1px solid var(--border-color);
}

.logo-icon-container {
  width: 36px;
  height: 36px;
  background-color: var(--primary-light);
  border-radius: var(--radius-sm);
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--primary-color);
}

.brand-text {
  display: flex;
  flex-direction: column;
}

.brand-name {
  font-weight: 700;
  font-size: 1.15rem;
  color: var(--text-primary);
  line-height: 1.1;
}

.brand-sub {
  font-size: 0.65rem;
  letter-spacing: 0.1em;
  color: var(--text-muted);
  font-weight: 700;
}

.sidebar-nav {
  padding: var(--space-lg) var(--space-md);
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
}

.new-doc-btn {
  width: 100%;
  padding: 12px;
}

.nav-links {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.nav-link {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  padding: 10px var(--space-md);
  font-size: var(--font-size-sm);
  font-weight: 600;
  color: var(--text-secondary);
  border-radius: var(--radius-md);
  transition: all var(--transition-fast);
}

.nav-link:hover {
  background-color: var(--bg-input);
  color: var(--text-primary);
}

.nav-link.active {
  background-color: var(--primary-light);
  color: var(--primary-color);
}

.sidebar-history-section {
  flex: 1;
  display: flex;
  flex-direction: column;
  padding: 0 var(--space-md);
  overflow: hidden;
}

.history-title {
  font-size: var(--font-size-xs);
  font-weight: 700;
  text-transform: uppercase;
  color: var(--text-muted);
  letter-spacing: 0.05em;
  margin-bottom: var(--space-sm);
  display: block;
}

.history-search-container {
  position: relative;
  margin-bottom: var(--space-sm);
}

.history-search-icon {
  position: absolute;
  left: 10px;
  top: 50%;
  transform: translateY(-50%);
  color: var(--text-muted);
}

.history-search-input {
  width: 100%;
  padding: 6px 10px 6px 30px;
  font-size: var(--font-size-xs);
  border-radius: var(--radius-sm);
  border: 1px solid var(--border-color);
  background-color: var(--bg-input);
  color: var(--text-primary);
}

.history-search-input:focus {
  outline: none;
  border-color: var(--border-focus);
}

.history-list {
  flex: 1;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
  gap: var(--space-xs);
}

.history-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: var(--space-sm);
  border-radius: var(--radius-sm);
  cursor: pointer;
  transition: all var(--transition-fast);
}

.history-item:hover {
  background-color: var(--bg-input);
}

.history-item-content {
  display: flex;
  flex-direction: column;
  overflow: hidden;
  flex: 1;
}

.history-item-name {
  font-size: var(--font-size-sm);
  font-weight: 500;
  color: var(--text-primary);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.history-item-date {
  font-size: var(--font-size-xs);
  color: var(--text-muted);
}

.history-delete-btn {
  background: none;
  border: none;
  color: var(--text-muted);
  cursor: pointer;
  padding: 4px;
  border-radius: 4px;
  opacity: 0;
  transition: all var(--transition-fast);
}

.history-item:hover .history-delete-btn {
  opacity: 1;
}

.history-delete-btn:hover {
  background-color: rgba(225, 29, 72, 0.08);
  color: #e11d48;
}

.history-empty {
  font-size: var(--font-size-xs);
  color: var(--text-muted);
  text-align: center;
  padding: var(--space-md) 0;
}

.sidebar-footer {
  padding: var(--space-md);
  border-top: 1px solid var(--border-color);
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.footer-link {
  font-size: var(--font-size-xs);
  color: var(--text-secondary);
  font-weight: 600;
  display: flex;
  align-items: center;
  gap: var(--space-xs);
}

.footer-link:hover {
  color: var(--primary-color);
}
</style>
