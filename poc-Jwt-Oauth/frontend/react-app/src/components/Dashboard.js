import React, { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import axios from 'axios';
import './Dashboard.css';

const Dashboard = () => {
  const { user, logout } = useAuth();
  const [profile, setProfile] = useState(null);
  const [adminMessage, setAdminMessage] = useState('');
  const [loading, setLoading] = useState(false);
  const API_URL = process.env.REACT_APP_API_URL || 'http://localhost:5000';

  useEffect(() => {
    fetchProfile();
  }, []);

  const fetchProfile = async () => {
    try {
      const response = await axios.get(`${API_URL}/api/protected/profile`);
      setProfile(response.data);
    } catch (error) {
      console.error('Failed to fetch profile:', error);
    }
  };

  const testAdminEndpoint = async () => {
    setLoading(true);
    try {
      const response = await axios.get(`${API_URL}/api/protected/admin`);
      setAdminMessage(response.data.message);
    } catch (error) {
      setAdminMessage(error.response?.data?.message || 'Access denied');
    }
    setLoading(false);
  };

  const testPublicEndpoint = async () => {
    try {
      const response = await axios.get(`${API_URL}/api/protected/public`);
      alert(response.data.message);
    } catch (error) {
      alert('Failed to access public endpoint');
    }
  };

  return (
    <div className="dashboard-container">
      <div className="dashboard-header">
        <h1>Dashboard</h1>
        <button onClick={logout} className="logout-btn">
          Logout
        </button>
      </div>

      <div className="dashboard-content">
        <div className="user-info">
          <h2>User Information</h2>
          {profile && (
            <div className="info-card">
              <p><strong>Username:</strong> {profile.username}</p>
              <p><strong>Email:</strong> {profile.email}</p>
              <p><strong>User ID:</strong> {profile.userId}</p>
              <p><strong>Roles:</strong> {profile.roles?.join(', ')}</p>
              <p><strong>Authenticated:</strong> {profile.isAuthenticated ? 'Yes' : 'No'}</p>
              <p><strong>Auth Type:</strong> {user?.isOAuthUser ? 'OAuth' : 'Local'}</p>
            </div>
          )}
        </div>

        <div className="actions">
          <h2>Test Endpoints</h2>
          <div className="action-buttons">
            <button onClick={testPublicEndpoint} className="action-btn public">
              Test Public Endpoint
            </button>
            <button 
              onClick={testAdminEndpoint} 
              className="action-btn admin"
              disabled={loading}
            >
              {loading ? 'Testing...' : 'Test Admin Endpoint'}
            </button>
          </div>
          {adminMessage && (
            <div className={`message ${adminMessage.includes('denied') ? 'error' : 'success'}`}>
              {adminMessage}
            </div>
          )}
        </div>

        <div className="token-info">
          <h2>Token Information</h2>
          <div className="info-card">
            <p><strong>Access Token:</strong> <span className="token-text">{localStorage.getItem('accessToken')?.substring(0, 50)}...</span></p>
            <p><strong>Refresh Token:</strong> <span className="token-text">{localStorage.getItem('refreshToken')?.substring(0, 50)}...</span></p>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Dashboard;