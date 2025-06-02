import React, { useState, useEffect } from 'react';
import { Container, Grid, TextField, Typography, Card, CardContent } from '@mui/material';
import { HubConnectionBuilder, HttpTransportType } from '@microsoft/signalr';
import { ToastContainer, toast } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import AuthEndpoints from './components/AuthEndpoints';
import PlayerEndpoints from './components/PlayerEndpoints';
import BetEndpoints from './components/BetEndpoints';

function App() {
  const [jwtToken, setJwtToken] = useState('');
  const [connection, setConnection] = useState(null);
  const [messages, setMessages] = useState([]);

  const initializeSignalRConnection = (token) => {
    console.log('Initializing SignalR connection with token:', token);
    try {
      const newConnection = new HubConnectionBuilder()
        .withUrl('http://localhost:8080/hubs/user', {
          accessTokenFactory: () => token,
          transport: HttpTransportType.WebSockets,
          skipNegotiation: true
        })
        .withAutomaticReconnect()
        .build();

      newConnection
        .start()
        .then(() => {
          console.log('Connected to SignalR hub');
          setConnection(newConnection);

          newConnection.on('wallet-transaction-update', (transaction) => {
            console.log('Wallet transaction received:', transaction);
            if (!transaction) {
              console.error('Invalid transaction data received');
              return;
            }

            // Map transaction type to appropriate display
            let emoji, amountColor, typeLabel, displayAmount;

            switch (transaction.typeId) {
              case 1: // Debit
                emoji = '💸';
                amountColor = 'red';
                typeLabel = 'Debit';
                displayAmount = `-${Math.abs(transaction.amount)}`;
                break;
              case 2: // Credit
                emoji = '💰';
                amountColor = 'green';
                typeLabel = 'Credit';
                displayAmount = `+${Math.abs(transaction.amount)}`;
                break;
              case 3: // Checkpoint
                emoji = '🔄';
                amountColor = 'blue';
                typeLabel = 'Checkpoint';
                displayAmount = `${transaction.amount}`;
                break;
              default:
                emoji = '💱';
                amountColor = 'gray';
                typeLabel = 'Unknown';
                displayAmount = `${transaction.amount}`;
            }

            const message = `
              <div style="display: flex; align-items: center; justify-content: space-between">
                <span>${emoji} <strong>Wallet</strong> #${transaction.walletId.substring(0, 6)} <small style="color: #666">(${typeLabel})</small></span>
                <span style="color:${amountColor}; font-weight: bold">${displayAmount}</span>
              </div>
            `;
            setMessages(prev => [...prev, message]);
            toast.info(() => <div dangerouslySetInnerHTML={{ __html: message }} />, {
              closeButton: true,
              autoClose: 5000,
            });
          });

          newConnection.on('bet-update', (bet) => {
            console.log('Bet update received:', bet);
            if (!bet) {
              console.error('Invalid bet data received');
              return;
            }
            const message = `
              <div style="display: flex; align-items: center; gap: 8px">
                <span style="background: #2196f3; color: white; padding: 2px 6px; border-radius: 4px; font-size: 12px">🔄 UPDATED</span>
                <span><strong>Bet</strong> #${bet.id?.substring(0, 6)}</span>
              </div>
            `;
            setMessages(prev => [...prev, message]);
            toast.info(() => <div dangerouslySetInnerHTML={{ __html: message }} />, {
              closeButton: true,
              autoClose: 5000,
            });
          });

          newConnection.on('bet-cancelled', (bet) => {
            console.log('Bet cancelled received:', bet);
            if (!bet) {
              console.error('Invalid bet data received');
              return;
            }
            const message = `
              <div style="display: flex; align-items: center; gap: 8px">
                <span style="background: #ff9800; color: white; padding: 2px 6px; border-radius: 4px; font-size: 12px">❌ CANCELLED</span>
                <span><strong>Bet</strong> #${bet.id?.substring(0, 6)}</span>
              </div>
            `;
            setMessages(prev => [...prev, message]);
            toast.warning(() => <div dangerouslySetInnerHTML={{ __html: message }} />, {
              closeButton: true,
              autoClose: 5000,
            });
          });

          newConnection.on('bet-settled', (bet) => {
            console.log('Bet settled received:', bet);
            if (!bet) {
              console.error('Invalid bet data received');
              return;
            }
            const message = `
              <div style="display: flex; align-items: center; gap: 8px">
                <span style="background: #4caf50; color: white; padding: 2px 6px; border-radius: 4px; font-size: 12px">✅ SETTLED</span>
                <span><strong>Bet</strong> #${bet.id?.substring(0, 6)}</span>
              </div>
            `;
            setMessages(prev => [...prev, message]);
            toast.success(() => <div dangerouslySetInnerHTML={{ __html: message }} />, {
              closeButton: true,
              autoClose: 5000,
            });
          });
        })
        .catch((err) => {
          console.error('SignalR connection failed:', err);
          toast.error('Failed to connect to SignalR hub');
        });
    } catch (error) {
      console.error('Error setting up SignalR connection:', error);
      toast.error('Failed to setup SignalR connection');
    }
  };

  useEffect(() => {
    if (jwtToken && !connection) {
      initializeSignalRConnection(jwtToken);
    }
  }, [jwtToken]);

  return (
    <Container style={{ padding: '20px', fontFamily: 'Arial, sans-serif' }}>
      <ToastContainer position="top-right" autoClose={5000} />
      <Typography variant="h4" gutterBottom>
        API Testing Dashboard
      </Typography>
      <Grid container spacing={3}>
        <Grid item xs={12}>
          <TextField
            label="JWT Token"
            fullWidth
            value={jwtToken}
            onChange={(e) => setJwtToken(e.target.value)}
          />
        </Grid>
        <Grid item xs={12} md={4}>
          <AuthEndpoints jwtToken={jwtToken} setJwtToken={setJwtToken} />
        </Grid>
        <Grid item xs={12} md={4}>
          <PlayerEndpoints jwtToken={jwtToken} />
        </Grid>
        <Grid item xs={12} md={4}>
          <BetEndpoints jwtToken={jwtToken} />
        </Grid>
        <Grid item xs={12}>
          {messages.length > 0 && (
            <Card>
              <CardContent>
                <Typography variant="h6">SignalR Messages</Typography>
                {messages.map((msg, idx) => (
                  <Typography key={idx} variant="body2">{msg}</Typography>
                ))}
              </CardContent>
            </Card>
          )}
        </Grid>
      </Grid>
    </Container>
  );
}

export default App;
