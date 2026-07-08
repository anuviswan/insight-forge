<script setup lang="ts">
import { useRouter } from "vue-router";
import { useAuthStore } from "../stores/auth";

const router = useRouter();
const authStore = useAuthStore();

function handleLoginClick() {
  router.push({ name: "Login" });
}

function handleSignupClick() {
  router.push({ name: "Register" });
}

function handleDashboardClick() {
  router.push({ name: "Blogger" });
}
</script>

<template>
  <div class="landing-container">
    <!-- Navigation Bar -->
    <nav class="navbar">
      <div class="nav-content">
        <div class="logo-section">
          <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" class="logo-icon">
            <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path>
            <polyline points="14 2 14 8 20 8"></polyline>
            <line x1="16" y1="13" x2="8" y2="13"></line>
            <line x1="16" y1="17" x2="8" y2="17"></line>
            <polyline points="10 9 9 9 8 9"></polyline>
          </svg>
          <span class="logo-text">Insight Forge</span>
        </div>
        <div class="nav-links">
          <button v-if="authStore.isAuthenticated" @click="handleDashboardClick" class="nav-link">Dashboard</button>
          <button v-if="!authStore.isAuthenticated" @click="handleLoginClick" class="nav-link login-link">Log In</button>
          <button v-if="!authStore.isAuthenticated" @click="handleSignupClick" class="nav-link signup-link">Sign Up</button>
        </div>
      </div>
    </nav>

    <!-- Hero Section -->
    <section class="hero">
      <div class="hero-content">
        <h1 class="hero-title">AI-Powered Content Generation</h1>
        <p class="hero-subtitle">Create professional blog posts, summaries, and research documents powered by advanced AI orchestration</p>
        
        <div class="hero-features">
          <div class="feature">
            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"></path>
            </svg>
            <h3>Blog Generation</h3>
            <p>Generate comprehensive blog posts with research, citations, and formatting</p>
          </div>
          
          <div class="feature">
            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"></path>
              <circle cx="9" cy="10" r="1"></circle>
              <circle cx="12" cy="10" r="1"></circle>
              <circle cx="15" cy="10" r="1"></circle>
            </svg>
            <h3>Summarization</h3>
            <p>Summarize multiple references into concise, actionable insights</p>
          </div>
          
          <div class="feature">
            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M12 2v20M2 12h20"></path>
              <circle cx="12" cy="12" r="10"></circle>
            </svg>
            <h3>AI Orchestration</h3>
            <p>Provider-agnostic AI integration with intelligent workflows</p>
          </div>
        </div>

        <div class="hero-buttons">
          <button @click="handleSignupClick" class="btn btn-primary btn-large">Get Started Free</button>
          <button @click="handleLoginClick" class="btn btn-secondary btn-large">Sign In</button>
        </div>
      </div>
      
      <div class="hero-visual">
        <div class="floating-card card-1">
          <div class="card-header">Blog Post</div>
          <div class="card-content">Generated content...</div>
        </div>
        <div class="floating-card card-2">
          <div class="card-header">Summary</div>
          <div class="card-content">Key insights...</div>
        </div>
        <div class="floating-card card-3">
          <div class="card-header">Research</div>
          <div class="card-content">Findings...</div>
        </div>
      </div>
    </section>

    <!-- Features Section -->
    <section class="features-section">
      <h2 class="section-title">Powerful Features</h2>
      <div class="features-grid">
        <div class="feature-card">
          <div class="feature-icon">
            <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <circle cx="12" cy="12" r="10"></circle>
              <polyline points="12 6 12 12 16 14"></polyline>
            </svg>
          </div>
          <h3>Fast Generation</h3>
          <p>Create high-quality content in minutes, not hours</p>
        </div>

        <div class="feature-card">
          <div class="feature-icon">
            <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M12 22c5.523 0 10-4.477 10-10S17.523 2 12 2 2 6.477 2 12s4.477 10 10 10z"></path>
              <path d="M12 6v6l4 2"></path>
            </svg>
          </div>
          <h3>Smart Research</h3>
          <p>Intelligent research workflows with verified sources</p>
        </div>

        <div class="feature-card">
          <div class="feature-icon">
            <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M12 2a10 10 0 1 0 10 10H12V2"></path>
            </svg>
          </div>
          <h3>Flexible</h3>
          <p>Customize tone, style, length, and audience</p>
        </div>

        <div class="feature-card">
          <div class="feature-icon">
            <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <rect x="3" y="3" width="18" height="18" rx="2"></rect>
              <path d="M9 11l3 3L22 4"></path>
            </svg>
          </div>
          <h3>Quality Assured</h3>
          <p>Automatic review for grammar, accuracy, and completeness</p>
        </div>
      </div>
    </section>

    <!-- CTA Section -->
    <section class="cta-section">
      <div class="cta-content">
        <h2>Ready to transform your content creation?</h2>
        <p>Join thousands of creators using Insight Forge to generate high-quality content</p>
        <button @click="handleSignupClick" class="btn btn-primary btn-large">Start For Free</button>
      </div>
    </section>

    <!-- Footer -->
    <footer class="footer">
      <div class="footer-content">
        <div class="footer-section">
          <h4>Insight Forge</h4>
          <p>AI-powered content generation platform</p>
        </div>
        <div class="footer-section">
          <h4>Legal</h4>
          <ul>
            <li><a href="#">Privacy Policy</a></li>
            <li><a href="#">Terms of Service</a></li>
          </ul>
        </div>
        <div class="footer-section">
          <h4>Support</h4>
          <ul>
            <li><a href="#">Documentation</a></li>
            <li><a href="#">Contact</a></li>
          </ul>
        </div>
      </div>
      <div class="footer-bottom">
        <p>&copy; 2026 Insight Forge. All rights reserved.</p>
      </div>
    </footer>
  </div>
</template>

<style scoped>
:root {
  --primary-color: #3b82f6;
  --primary-light: #dbeafe;
  --text-primary: #1f2937;
  --text-secondary: #6b7280;
  --bg-light: #f9fafb;
  --border-color: #e5e7eb;
}

.dark {
  --text-primary: #f3f4f6;
  --text-secondary: #d1d5db;
  --bg-light: #111827;
  --border-color: #374151;
}

.landing-container {
  min-height: 100vh;
  background-color: var(--bg-light);
  color: var(--text-primary);
}

/* Navbar */
.navbar {
  background-color: white;
  border-bottom: 1px solid var(--border-color);
  position: sticky;
  top: 0;
  z-index: 50;
}

.dark .navbar {
  background-color: #1f2937;
}

.nav-content {
  max-width: 1200px;
  margin: 0 auto;
  padding: 1rem 2rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.logo-section {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  font-weight: 700;
  font-size: 1.25rem;
  color: var(--primary-color);
  text-decoration: none;
}

.logo-icon {
  color: var(--primary-color);
}

.logo-text {
  background: linear-gradient(135deg, #3b82f6 0%, #1e40af 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.nav-links {
  display: flex;
  gap: 1rem;
  align-items: center;
}

.nav-link {
  padding: 0.5rem 1rem;
  border: none;
  background: none;
  cursor: pointer;
  font-weight: 500;
  color: var(--text-secondary);
  transition: color 0.2s;
}

.nav-link:hover {
  color: var(--primary-color);
}

.login-link {
  color: var(--text-secondary);
}

.signup-link {
  background-color: var(--primary-color);
  color: white;
  border-radius: 0.5rem;
  padding: 0.5rem 1.5rem;
}

.signup-link:hover {
  background-color: #2563eb;
}

/* Hero Section */
.hero {
  max-width: 1200px;
  margin: 0 auto;
  padding: 4rem 2rem;
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 4rem;
  align-items: center;
}

@media (max-width: 768px) {
  .hero {
    grid-template-columns: 1fr;
    padding: 2rem 1rem;
  }
}

.hero-content {
  display: flex;
  flex-direction: column;
  gap: 2rem;
}

.hero-title {
  font-size: 2.5rem;
  font-weight: 800;
  line-height: 1.2;
  color: var(--text-primary);
}

.hero-subtitle {
  font-size: 1.125rem;
  color: var(--text-secondary);
  line-height: 1.6;
}

.hero-features {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.feature {
  display: flex;
  gap: 1rem;
  align-items: flex-start;
}

.feature svg {
  width: 24px;
  height: 24px;
  color: var(--primary-color);
  flex-shrink: 0;
  margin-top: 0.25rem;
}

.feature h3 {
  font-weight: 600;
  margin-bottom: 0.25rem;
  color: var(--text-primary);
}

.feature p {
  font-size: 0.875rem;
  color: var(--text-secondary);
}

.hero-buttons {
  display: flex;
  gap: 1rem;
  flex-wrap: wrap;
}

.btn {
  padding: 0.75rem 1.5rem;
  border: none;
  border-radius: 0.5rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
  font-size: 1rem;
}

.btn-large {
  padding: 1rem 2rem;
  font-size: 1.0625rem;
}

.btn-primary {
  background-color: var(--primary-color);
  color: white;
}

.btn-primary:hover {
  background-color: #2563eb;
  transform: translateY(-2px);
}

.btn-secondary {
  background-color: white;
  color: var(--primary-color);
  border: 2px solid var(--primary-color);
}

.btn-secondary:hover {
  background-color: var(--primary-light);
}

/* Hero Visual */
.hero-visual {
  position: relative;
  height: 400px;
}

.floating-card {
  position: absolute;
  background: white;
  border: 1px solid var(--border-color);
  border-radius: 0.5rem;
  padding: 1.5rem;
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.1);
  animation: float 3s ease-in-out infinite;
}

.dark .floating-card {
  background-color: #1f2937;
}

.card-1 {
  width: 200px;
  top: 0;
  left: 0;
  animation-delay: 0s;
}

.card-2 {
  width: 180px;
  top: 120px;
  right: 20px;
  animation-delay: 1s;
}

.card-3 {
  width: 190px;
  bottom: 0;
  left: 60px;
  animation-delay: 2s;
}

.card-header {
  font-weight: 600;
  color: var(--primary-color);
  margin-bottom: 0.75rem;
  font-size: 0.875rem;
}

.card-content {
  color: var(--text-secondary);
  font-size: 0.8rem;
  line-height: 1.4;
}

@keyframes float {
  0%, 100% {
    transform: translateY(0px);
  }
  50% {
    transform: translateY(-20px);
  }
}

/* Features Section */
.features-section {
  max-width: 1200px;
  margin: 4rem auto;
  padding: 4rem 2rem;
}

.section-title {
  font-size: 2rem;
  font-weight: 800;
  text-align: center;
  margin-bottom: 3rem;
  color: var(--text-primary);
}

.features-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 2rem;
}

.feature-card {
  background: white;
  padding: 2rem;
  border-radius: 0.75rem;
  border: 1px solid var(--border-color);
  text-align: center;
  transition: all 0.3s;
}

.dark .feature-card {
  background-color: #1f2937;
}

.feature-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.1);
}

.feature-icon {
  width: 60px;
  height: 60px;
  background-color: var(--primary-light);
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto 1rem;
  color: var(--primary-color);
}

.feature-card h3 {
  font-weight: 600;
  margin-bottom: 0.75rem;
  color: var(--text-primary);
}

.feature-card p {
  color: var(--text-secondary);
  font-size: 0.875rem;
  line-height: 1.5;
}

/* CTA Section */
.cta-section {
  background: linear-gradient(135deg, #3b82f6 0%, #1e40af 100%);
  color: white;
  padding: 4rem 2rem;
  text-align: center;
}

.cta-content h2 {
  font-size: 2rem;
  font-weight: 800;
  margin-bottom: 1rem;
}

.cta-content p {
  font-size: 1.125rem;
  margin-bottom: 2rem;
  opacity: 0.9;
}

/* Footer */
.footer {
  background-color: #1f2937;
  color: #f3f4f6;
  padding: 3rem 2rem 1rem;
  margin-top: 4rem;
}

.footer-content {
  max-width: 1200px;
  margin: 0 auto;
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 2rem;
  margin-bottom: 2rem;
}

.footer-section h4 {
  font-weight: 600;
  margin-bottom: 1rem;
  color: white;
}

.footer-section p {
  color: #d1d5db;
  font-size: 0.875rem;
}

.footer-section ul {
  list-style: none;
  padding: 0;
}

.footer-section li {
  margin-bottom: 0.5rem;
}

.footer-section a {
  color: #d1d5db;
  text-decoration: none;
  font-size: 0.875rem;
  transition: color 0.2s;
}

.footer-section a:hover {
  color: white;
}

.footer-bottom {
  text-align: center;
  padding-top: 2rem;
  border-top: 1px solid #374151;
  color: #9ca3af;
  font-size: 0.875rem;
}
</style>
