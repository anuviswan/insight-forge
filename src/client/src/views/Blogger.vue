<script setup lang="ts">
import { ref } from 'vue';
import { useDocumentsStore } from '../stores/documents';

const documentsStore = useDocumentsStore();

const topic = ref('');
const audience = ref('');
const writingStyle = ref('');
const showFabAlert = ref(false);

async function handleGenerate() {
  if (!topic.value.trim()) return;
  try {
    await documentsStore.generateBlogPost(topic.value, audience.value, writingStyle.value);
  } catch (err) {
    console.error('Failed to generate post:', err);
  }
}

function handleCopy() {
  if (documentsStore.activePost) {
    navigator.clipboard.writeText(documentsStore.activePost.content);
    alert('Markdown copied to clipboard!');
  }
}

function handleDownload() {
  if (documentsStore.activePost) {
    const blob = new Blob([documentsStore.activePost.content], { type: 'text/markdown' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `${documentsStore.activePost.title.toLowerCase().replace(/\s+/g, '-')}.md`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
  }
}

function handleDelete() {
  if (confirm('Are you sure you want to delete this preview?')) {
    documentsStore.activePost = null;
  }
}
</script>

<template>
  <div class="blogger-container animate-fade-in">
    <!-- Main Generator Form -->
    <div class="generator-card card">
      <h1 class="generator-title">What's on your mind?</h1>
      <p class="generator-subtitle">Enter a topic and our AI will craft a high-quality Markdown blog post for you.</p>
      
      <form @submit.prevent="handleGenerate" class="input-form-wrapper">
        <div class="form-group">
          <label for="topic-input" class="form-label">Topic</label>
          <input
            id="topic-input"
            type="text"
            v-model="topic"
            placeholder="e.g., The Future of AI in Design"
            class="form-control generator-input"
            :disabled="documentsStore.loading"
          />
        </div>

        <div class="form-row">
          <div class="form-group form-group-half">
            <label for="audience-input" class="form-label">Intended Audience</label>
            <input
              id="audience-input"
              type="text"
              v-model="audience"
              placeholder="e.g., Product managers, Developers"
              class="form-control"
              :disabled="documentsStore.loading"
            />
          </div>

          <div class="form-group form-group-half">
            <label for="style-input" class="form-label">Writing Style/Tone</label>
            <input
              id="style-input"
              type="text"
              v-model="writingStyle"
              placeholder="e.g., Professional, Casual, Technical"
              class="form-control"
              :disabled="documentsStore.loading"
            />
          </div>
        </div>

        <button type="submit" class="btn btn-primary generate-btn" :disabled="documentsStore.loading || !topic.trim()">
          <span v-if="documentsStore.loading" class="spinner"></span>
          <span v-else>Generate</span>
        </button>
      </form>
    </div>

    <!-- Active Preview Card -->
    <div v-if="documentsStore.activePost" class="preview-card card animate-fade-in">
      <div class="preview-header">
        <div class="preview-meta-left">
          <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" class="doc-icon">
            <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path>
            <polyline points="14 2 14 8 20 8"></polyline>
          </svg>
          <span class="preview-filename">PREVIEW_POST.MD</span>
        </div>
        <div class="preview-actions">
          <button class="btn btn-secondary action-btn" @click="handleCopy">
            <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <rect x="9" y="9" width="13" height="13" rx="2" ry="2"></rect>
              <path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1"></path>
            </svg>
            Copy
          </button>
          <button class="btn btn-primary action-btn" @click="handleDownload">
            <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path>
              <polyline points="7 10 12 15 17 10"></polyline>
              <line x1="12" y1="15" x2="12" y2="3"></line>
            </svg>
            Download
          </button>
        </div>
      </div>

      <div class="preview-content">
        <h2 class="content-title">{{ documentsStore.activePost.title }}</h2>
        
        <p class="content-paragraph">The intersection of artificial intelligence and creative design is no longer a futuristic concept—it's the present reality of our industry. As we look ahead, the role of the designer is shifting from being the primary executor to becoming a creative director of algorithmic systems.</p>

        <!-- Preview Image -->
        <div class="preview-image-container" v-if="documentsStore.activePost.imageUrl">
          <img :src="documentsStore.activePost.imageUrl" alt="Preview Visual" class="preview-image" />
        </div>

        <h3 class="content-heading">1. Generative Co-creation</h3>
        <p class="content-paragraph">AI tools like Midjourney and DALL-E have already disrupted the visual ideation process. However, the true transformation lies in <strong>generative design systems</strong> that can iterate through thousands of layout variations based on user data and accessibility requirements in real-time.</p>

        <blockquote class="content-quote">
          "Design is not just what it looks like and feels like. Design is how it works—and now, how it learns."
        </blockquote>

        <h3 class="content-heading">2. The End of Repetitive Tasks</h3>
        <p class="content-paragraph">From resizing assets to generating colour palettes that meet contrast ratios, AI is liberating designers from the mechanical parts of the job. This shift allows human creators to focus on <strong>empathy, storytelling, and strategic brand thinking</strong>—areas where machines still struggle to find nuance.</p>

        <p class="content-paragraph">As we move into 2025, the successful designer will be the one who masters the 'prompt'—not just as a text string, but as a deep understanding of how to guide latent spaces toward meaningful human outcomes.</p>
      </div>

      <div class="preview-footer">
        <span class="word-stats">Words: {{ documentsStore.activePost.wordCount }} | Est. Read: {{ documentsStore.activePost.readTime }}</span>
        
        <div class="footer-icons">
          <button class="footer-icon-btn" aria-label="Share document">
            <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <circle cx="18" cy="5" r="3"></circle>
              <circle cx="6" cy="12" r="3"></circle>
              <circle cx="18" cy="19" r="3"></circle>
              <line x1="8.59" y1="13.51" x2="15.42" y2="17.49"></line>
              <line x1="15.41" y1="6.51" x2="8.59" y2="10.49"></line>
            </svg>
          </button>
          <button class="footer-icon-btn delete" @click="handleDelete" aria-label="Delete document">
            <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <polyline points="3 6 5 6 21 6"></polyline>
              <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path>
              <line x1="10" y1="11" x2="10" y2="17"></line>
              <line x1="14" y1="11" x2="14" y2="17"></line>
            </svg>
          </button>
        </div>
      </div>
    </div>

    <!-- Floating Action Button (FAB) -->
    <div class="fab-container">
      <button class="fab-btn" @click="showFabAlert = true" aria-label="Quick action edit button">
        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
          <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"></path>
          <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"></path>
        </svg>
      </button>
      <div v-if="showFabAlert" class="fab-tooltip animate-fade-in">
        <span>Quick Editing Mode Activated</span>
        <button class="tooltip-close" @click="showFabAlert = false">×</button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.blogger-container {
  max-width: 860px;
  margin: 0 auto;
  width: 100%;
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
  padding-bottom: 80px;
}

.breadcrumb-tabs {
  display: flex;
  gap: var(--space-md);
  border-bottom: 1px solid var(--border-color);
  padding-bottom: var(--space-sm);
}

.tab-btn {
  background: none;
  border: none;
  font-size: var(--font-size-base);
  font-weight: 600;
  color: var(--text-secondary);
  cursor: pointer;
  padding: 6px 12px;
  position: relative;
  transition: color var(--transition-fast);
}

.tab-btn:hover {
  color: var(--text-primary);
}

.tab-btn.active {
  color: var(--primary-color);
}

.tab-btn.active::after {
  content: '';
  position: absolute;
  bottom: -9px;
  left: 0;
  right: 0;
  height: 2px;
  background-color: var(--primary-color);
}

.generator-card {
  text-align: center;
  padding: var(--space-xl);
}

.generator-title {
  font-size: var(--font-size-xxl);
  font-weight: 700;
  margin-bottom: var(--space-xs);
}

.generator-subtitle {
  font-size: var(--font-size-sm);
  color: var(--text-secondary);
  margin-bottom: var(--space-lg);
}

.input-form-wrapper {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: var(--space-xs);
}

.form-label {
  font-size: var(--font-size-sm);
  font-weight: 600;
  color: var(--text-primary);
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: var(--space-md);
}

.form-group-half {
  width: 100%;
}

.generator-input {
  width: 100%;
}

.generate-btn {
  width: 100%;
  padding: var(--space-md) var(--space-lg);
}

@media (max-width: 640px) {
  .form-row {
    grid-template-columns: 1fr;
  }
}

.preview-card {
  padding: 0;
  overflow: hidden;
}

.preview-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: var(--space-md) var(--space-lg);
  border-bottom: 1px solid var(--border-color);
  background-color: var(--bg-input);
}

.preview-meta-left {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  color: var(--text-secondary);
}

.doc-icon {
  color: var(--primary-color);
}

.preview-filename {
  font-family: var(--font-mono);
  font-weight: 700;
  font-size: var(--font-size-sm);
}

.preview-actions {
  display: flex;
  gap: var(--space-sm);
}

.action-btn {
  padding: 8px var(--space-md);
  font-size: var(--font-size-xs);
}

.preview-content {
  padding: var(--space-xl);
  color: var(--text-secondary);
  line-height: 1.7;
}

.content-title {
  margin-bottom: var(--space-md);
}

.content-paragraph {
  margin-bottom: var(--space-md);
}

.preview-image-container {
  width: 100%;
  max-height: 240px;
  border-radius: var(--radius-md);
  overflow: hidden;
  margin-bottom: var(--space-lg);
}

.preview-image {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.content-heading {
  margin-top: var(--space-lg);
  margin-bottom: var(--space-sm);
}

.content-quote {
  border-left: 4px solid var(--primary-color);
  padding: var(--space-sm) var(--space-md);
  font-style: italic;
  background-color: var(--bg-input);
  border-radius: 0 var(--radius-sm) var(--radius-sm) 0;
  margin-bottom: var(--space-md);
}

.preview-footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: var(--space-md) var(--space-lg);
  border-top: 1px solid var(--border-color);
  background-color: var(--bg-input);
}

.word-stats {
  font-size: var(--font-size-xs);
  color: var(--text-muted);
  font-weight: 500;
}

.footer-icons {
  display: flex;
  gap: var(--space-sm);
}

.footer-icon-btn {
  background: none;
  border: none;
  color: var(--text-secondary);
  cursor: pointer;
  padding: 6px;
  border-radius: var(--radius-sm);
  transition: all var(--transition-fast);
}

.footer-icon-btn:hover {
  background-color: var(--border-color);
  color: var(--text-primary);
}

.footer-icon-btn.delete:hover {
  background-color: rgba(225, 29, 72, 0.08);
  color: #e11d48;
}

/* FAB button styling */
.fab-container {
  position: fixed;
  bottom: var(--space-xl);
  right: var(--space-xl);
  z-index: 99;
  display: flex;
  align-items: center;
  gap: var(--space-sm);
}

.fab-btn {
  width: 56px;
  height: 56px;
  border-radius: var(--radius-full);
  background-color: var(--primary-color);
  color: #ffffff;
  border: none;
  box-shadow: var(--shadow-primary);
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all var(--transition-fast);
}

.fab-btn:hover {
  background-color: var(--primary-hover);
  transform: translateY(-2px) scale(1.05);
}

.fab-tooltip {
  background-color: var(--text-primary);
  color: var(--bg-card);
  padding: var(--space-sm) var(--space-md);
  border-radius: var(--radius-md);
  font-size: var(--font-size-xs);
  font-weight: 600;
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  box-shadow: var(--shadow-lg);
}

.tooltip-close {
  background: none;
  border: none;
  color: inherit;
  font-size: var(--font-size-base);
  cursor: pointer;
  line-height: 1;
}

.spinner {
  width: 18px;
  height: 18px;
  border: 2px solid rgba(255, 255, 255, 0.3);
  border-radius: 50%;
  border-top-color: #ffffff;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}
</style>
