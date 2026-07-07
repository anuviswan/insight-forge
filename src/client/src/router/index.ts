import { createRouter, createWebHistory } from "vue-router";
import { useAuthStore } from "../stores/auth";

// Lazy load route components as per rules
const Login = () => import("../views/Login.vue");
const Register = () => import("../views/Register.vue");
const VerifyEmail = () => import("../views/VerifyEmail.vue");
const Blogger = () => import("../views/Blogger.vue");
const Summarizer = () => import("../views/Summarizer.vue");
const Profile = () => import("../views/Profile.vue");

const routes = [
  {
    path: "/login",
    name: "Login",
    component: Login,
    meta: { requiresGuest: true }
  },
  {
    path: "/register",
    name: "Register",
    component: Register,
    meta: { requiresGuest: true }
  },
  {
    path: "/verify-email",
    name: "VerifyEmail",
    component: VerifyEmail,
    meta: { requiresGuest: true }
  },
  {
    path: "/blogger",
    name: "Blogger",
    component: Blogger,
    meta: { requiresAuth: true }
  },
  {
    path: "/summariser",
    name: "Summariser",
    component: Summarizer,
    meta: { requiresAuth: true }
  },
  {
    path: "/profile",
    name: "Profile",
    component: Profile,
    meta: { requiresAuth: true }
  },
  {
    path: "/:pathMatch(.*)*",
    redirect: "/blogger"
  }
];

export const router = createRouter({
  history: createWebHistory(),
  routes
});

// Navigation guards
router.beforeEach((to, _from, next) => {
  const authStore = useAuthStore();
  const isAuthenticated = authStore.isAuthenticated;

  if (to.meta.requiresAuth && !isAuthenticated) {
    next({ name: "Login" });
  } else if (to.meta.requiresGuest && isAuthenticated) {
    next({ name: "Blogger" });
  } else {
    next();
  }
});

export default router;
