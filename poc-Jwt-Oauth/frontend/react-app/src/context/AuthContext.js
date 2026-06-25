import React, { createContext, useState, useContext, useEffect } from 'react';
import axios from 'axios';

const AuthContext = createContext();

export const useAuth = () => useContext(AuthContext);

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);
  const [accessToken, setAccessToken] = useState(localStorage.getItem('accessToken'));
  const [refreshToken, setRefreshToken] = useState(localStorage.getItem('refreshToken'));

  const API_URL = process.env.REACT_APP_API_URL || 'http://localhost:5000';

  // Configure axios
  axios.interceptors.request.use(
    (config) => {
      if (accessToken) {
        config.headers.Authorization = `Bearer ${accessToken}`;
      }
      return config;
    },
    (error) => Promise.reject(error)
  );

  // Token refresh interceptor
  axios.interceptors.response.use(
    (response) => response,
    async (error) => {
      const originalRequest = error.config;
      if (error.response?.status === 401 && !originalRequest._retry) {
        originalRequest._retry = true;
        try {
          const response = await refreshAccessToken();
          if (response) {
            originalRequest.headers.Authorization = `Bearer ${response.accessToken}`;
            return axios(originalRequest);
          }
        } catch (refreshError) {
          logout();
        }
      }
      return Promise.reject(error);
    }
  );

  useEffect(() => {
    const storedUser = localStorage.getItem('user');
    if (storedUser) {
      setUser(JSON.parse(storedUser));
    }
    setLoading(false);
  }, []);

  const login = async (username, password) => {
    try {
      const response = await axios.post(`${API_URL}/api/auth/login`, {
        username,
        password
      });
      
      const { accessToken, refreshToken, ...userData } = response.data;
      setAuthData(accessToken, refreshToken, userData);
      return { success: true };
    } catch (error) {
      return { 
        success: false, 
        message: error.response?.data?.message || 'Login failed' 
      };
    }
  };

  const register = async (username, email, password) => {
    try {
      const response = await axios.post(`${API_URL}/api/auth/register`, {
        username,
        email,
        password
      });
      
      const { accessToken, refreshToken, ...userData } = response.data;
      setAuthData(accessToken, refreshToken, userData);
      return { success: true };
    } catch (error) {
      return { 
        success: false, 
        message: error.response?.data?.message || 'Registration failed' 
      };
    }
  };

  const refreshAccessToken = async () => {
    try {
      const response = await axios.post(`${API_URL}/api/auth/refresh`, {
        refreshToken
      });
      
      const { accessToken, refreshToken: newRefreshToken } = response.data;
      setAccessToken(accessToken);
      setRefreshToken(newRefreshToken || refreshToken);
      localStorage.setItem('accessToken', accessToken);
      if (newRefreshToken) {
        localStorage.setItem('refreshToken', newRefreshToken);
      }
      return { accessToken };
    } catch (error) {
      throw error;
    }
  };

  const setAuthData = (accessToken, refreshToken, userData) => {
    setAccessToken(accessToken);
    setRefreshToken(refreshToken);
    setUser(userData);
    localStorage.setItem('accessToken', accessToken);
    localStorage.setItem('refreshToken', refreshToken);
    localStorage.setItem('user', JSON.stringify(userData));
  };

  const logout = async () => {
    try {
      await axios.post(`${API_URL}/api/auth/logout`, { refreshToken });
    } catch (error) {
      console.error('Logout error:', error);
    }
    
    setAccessToken(null);
    setRefreshToken(null);
    setUser(null);
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
  };

  const oauthLogin = async (provider) => {
    window.location.href = `${API_URL}/api/auth/oauth/${provider}`;
  };

  const handleOAuthCallback = (token, refreshToken) => {
    // The token is passed as URL parameters from the backend redirect
    if (token) {
      setAuthData(token, refreshToken, {});
      // Fetch user profile
      fetchUserProfile();
    }
  };

  const fetchUserProfile = async () => {
    try {
      const response = await axios.get(`${API_URL}/api/protected/profile`);
      const userData = response.data;
      const storedUser = JSON.parse(localStorage.getItem('user') || '{}');
      setUser({ ...storedUser, ...userData });
      localStorage.setItem('user', JSON.stringify({ ...storedUser, ...userData }));
    } catch (error) {
      console.error('Failed to fetch user profile:', error);
    }
  };

  const value = {
    user,
    loading,
    login,
    register,
    logout,
    refreshAccessToken,
    oauthLogin,
    handleOAuthCallback,
    isAuthenticated: !!accessToken
  };

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
};