// Interfaces for API DTOs
export interface UserProfile {
  name: string;
  email: string;
  avatar: string;
  bio: string;
  isPro: boolean;
  roles: string[];
  preferences: {
    darkMode: boolean;
    autoSave: boolean;
    notifications: boolean;
  };
  security: {
    twoFactor: boolean;
  };
}

export interface AuthResponse {
  token: string;
  user: UserProfile;
}

export interface RegistrationResponse {
  success: boolean;
  userId?: string;
  message: string;
  errorCode?: string;
  validationErrors?: string[];
  verificationToken?: string;
}

export interface VerifyEmailResponse {
  success: boolean;
  message: string;
  errorCode?: string;
}

export interface SignInRequest {
  email: string;
  password: string;
  rememberMe?: boolean;
}

export interface SignInResponse {
  success: boolean;
  userId?: string;
  accessToken?: string;
  refreshToken?: string;
  message: string;
  errorCode?: string;
  validationErrors?: string[];
}

export interface BlogPost {
  title: string;
  content: string;
  wordCount: number;
  readTime: string;
  imageUrl: string;
}

export interface ResearchSummary {
  title: string;
  content: string;
  sourcesAnalyzed: number;
  readingTimeSaved: number;
  sizeKb: number;
}

// Initial mock data
const mockUser: UserProfile = {
  name: "Alex Rivers",
  email: "alex.rivers@markdownstudio.io",
  avatar: "https://images.unsplash.com/photo-1534528741775-53994a69daeb?auto=format&fit=crop&w=150&h=150&q=80",
  bio: "Technical writer and researcher focusing on the intersection of AI and human creativity. Managing over 50 projects in Markdown Studio.",
  isPro: true,
  roles: ["WRITER", "RESEARCHER"],
  preferences: {
    darkMode: false,
    autoSave: true,
    notifications: true
  },
  security: {
    twoFactor: false
  }
};

const mockRecentHistory = [
  { id: "1", title: "The Future of AI in Design", type: "blog", date: "2 hours ago" },
  { id: "2", title: "Quantum Computing Research", type: "summary", date: "2 hours ago" },
  { id: "3", title: "Quantum Computing Summary", type: "summary", date: "Yesterday" },
  { id: "4", title: "AI Ethics Discussion", type: "summary", date: "Yesterday" },
  { id: "5", title: "Web Development Trends", type: "blog", date: "Oct 24, 2023" }
];

// Get API base URL (from environment or default)
const API_BASE_URL = import.meta.env.VITE_API_URL || "http://localhost:5000/api";

// Centralized API service layer
export const api = {
  auth: {
    async register(email: string, password: string): Promise<RegistrationResponse> {
      try {
        const response = await fetch(`${API_BASE_URL}/auth/register`, {
          method: "POST",
          headers: {
            "Content-Type": "application/json"
          },
          body: JSON.stringify({
            email,
            password,
            confirmPassword: password
          })
        });

        const data = await response.json();
        return data;
      } catch (error) {
        console.error("Registration error:", error);
        return {
          success: false,
          message: "Failed to connect to server",
          errorCode: "NETWORK_ERROR"
        };
      }
    },

    async verifyEmail(token: string): Promise<VerifyEmailResponse> {
      try {
        const response = await fetch(`${API_BASE_URL}/auth/verify-email`, {
          method: "POST",
          headers: {
            "Content-Type": "application/json"
          },
          body: JSON.stringify({ token })
        });

        const data = await response.json();
        return data;
      } catch (error) {
        console.error("Email verification error:", error);
        return {
          success: false,
          message: "Failed to connect to server",
          errorCode: "NETWORK_ERROR"
        };
      }
    },

    async signIn(email: string, password: string, rememberMe: boolean = false): Promise<SignInResponse> {
      try {
        const response = await fetch(`${API_BASE_URL}/auth/sign-in`, {
          method: "POST",
          headers: {
            "Content-Type": "application/json"
          },
          body: JSON.stringify({
            email,
            password,
            rememberMe
          })
        });

        const data = await response.json();
        return data;
      } catch (error) {
        console.error("Sign-in error:", error);
        return {
          success: false,
          message: "Failed to connect to server",
          errorCode: "NETWORK_ERROR"
        };
      }
    },

    async login(email: string, _password?: string, provider?: "google" | "github"): Promise<AuthResponse> {
      // Simulate network latency
      await new Promise((resolve) => setTimeout(resolve, 600));
      return {
        token: "mock-jwt-token-12345",
        user: {
          ...mockUser,
          email: email || (provider ? `${provider}@social.com` : mockUser.email),
          name: email ? email.split("@")[0] : (provider ? `${provider} User` : mockUser.name)
        }
      };
    },

    async logout(): Promise<void> {
      await new Promise((resolve) => setTimeout(resolve, 300));
    }
  },

  blogger: {
    async generate(topic: string): Promise<BlogPost> {
      await new Promise((resolve) => setTimeout(resolve, 1500));

      const title = topic.trim() || "The Future of AI in Design";
      return {
        title,
        content: `# ${title}\n\nThe intersection of artificial intelligence and creative design is no longer a futuristic concept—it's the present reality of our industry. As we look ahead, the role of the designer is shifting from being the primary executor to becoming a creative director of algorithmic systems.\n\n### 1. Generative Co-creation\n\nAI tools like Midjourney and DALL-E have already disrupted the visual ideation process. However, the true transformation lies in **generative design systems** that can iterate through thousands of layout variations based on user data and accessibility requirements in real-time.\n\n> "Design is not just what it looks like and feels like. Design is how it works—and now, how it learns."\n\n### 2. The End of Repetitive Tasks\n\nFrom resizing assets to generating colour palettes that meet contrast ratios, AI is liberating designers from the mechanical parts of the job. This shift allows human creators to focus on **empathy, storytelling, and strategic brand thinking**—areas where machines still struggle to find nuance.\n\nAs we move into 2025, the successful designer will be the one who masters the 'prompt'—not just as a text string, but as a deep understanding of how to guide latent spaces toward meaningful human outcomes.`,
        wordCount: 432,
        readTime: "2m",
        imageUrl: "https://images.unsplash.com/photo-1618005182384-a83a8bd57fbe?auto=format&fit=crop&w=800&q=80"
      };
    }
  },

  summariser: {
    async summarise(urls: string[]): Promise<ResearchSummary> {
      await new Promise((resolve) => setTimeout(resolve, 1800));
      return {
        title: "Summary of Research: The Future of Quantum Computing",
        content: `### Executive Overview\n\nThis report synthesises information from provided sources regarding the trajectory of quantum processor development and its implications for cryptographic standards. Recent breakthroughs indicate a shortening timeline for practical quantum advantage in specific chemical simulation domains.\n\n### Key Insights & Bullet Points\n\n* **Error Mitigation:** New hybrid algorithms are reducing logical qubit overhead by 30% compared to 2023 benchmarks.\n* **Hardware Scaling:** Superconducting circuits remain the dominant architecture, though trapped-ion systems are showing superior coherence times.\n* **Market Impact:** Investment in quantum-safe encryption is projected to grow by 40% annually through 2028.\n\n### Technical Constraints\n\nCryogenic requirements remain the primary bottleneck for edge deployment. Current modular cooling units are still too voluminous for data centre rack standards, necessitating bespoke facility designs.\n\n### Next Steps\n\nResearchers should focus on cross-platform verification and the standardisation of quantum assembly languages (QASM) to ensure inter-operability between hardware vendors.`,
        sourcesAnalyzed: urls.filter((u) => u.trim().length > 0).length || 4,
        readingTimeSaved: 18,
        sizeKb: 1.2
      };
    }
  },

  history: {
    async fetchRecent(): Promise<typeof mockRecentHistory> {
      await new Promise((resolve) => setTimeout(resolve, 400));
      return [...mockRecentHistory];
    }
  }
};
