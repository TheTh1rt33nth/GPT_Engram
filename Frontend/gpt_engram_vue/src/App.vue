<template>
  <div class="container">
    <header class="header">
      <pre class="warhammer-title">
         IN THE GRIM DARKNESS OF THE FAR FUTURE,
         THERE IS ONLY CHAT...
      </pre>
      <h1>WARHAMMER 40,000 - Game Reports Chat</h1>
    </header>

    <div class="chat-window">
      <div
        v-for="(msg, index) in messages"
        :key="index"
        class="message"
        :class="msg.sender"
      >
        <strong>{{ msg.sender }}:</strong> {{ msg.text }}
      </div>
    </div>

    <div class="input-row">
      <input
        class="grim-input"
        v-model="userQuery"
        @keyup.enter="sendQuery"
        placeholder="Ask about a battle report..."
      />
      <button class="grim-button" @click="sendQuery">Send</button>
    </div>
  </div>
</template>

<script>
import axios from 'axios';

export default {
  name: 'App',
  data() {
    return {
      userQuery: '',
      messages: []
    };
  },
  methods: {
    async sendQuery() {
      if (!this.userQuery) return;

      this.messages.push({ sender: 'user', text: this.userQuery });

      try {
        const response = await axios.post('http://localhost:5086/api/chat/ask', {
          userQuery: this.userQuery
        });
        const botAnswer = response.data.answer;
        this.messages.push({ sender: 'assistant', text: botAnswer });
      } catch (err) {
        console.error(err);
        this.messages.push({
          sender: 'assistant',
          text: 'The Warp has intervened... an error occurred.'
        });
      }

      this.userQuery = '';
    }
  }
};
</script>

<style scoped>
.container {
  max-width: 800px;
  margin: 0 auto;
  padding: 1rem;
  background-color: #1a1a1a; 
  color: #e0e0e0; 
  font-family: Arial, sans-serif; 
  min-height: 100vh;
}

.header {
  text-align: center;
  margin-bottom: 1rem;
  border-bottom: 1px solid #444;
  padding-bottom: 0.5rem;
}

.warhammer-title {
  font-family: monospace;
  font-size: 0.9rem;
  line-height: 1.2rem;
  color: #aaa;
  margin-bottom: 0.5rem;
  white-space: pre;
  text-align: center;
}

h1 {
  font-family: 'Cinzel', serif;
  font-size: 1.8rem;
  color: #c8a060; 
  margin: 0;
}

.chat-window {
  border: 1px solid #444;
  background-color: #282828;
  min-height: 300px;
  padding: 1rem;
  margin-bottom: 1rem;
  overflow-y: auto;
  max-height: 400px;
}

.message {
  margin-bottom: 0.8rem;
}

.message.user {
  text-align: right;
  color: #ffa;
}

.message.assistant {
  text-align: left;
  color: #afa;
}

/* Input & Button */
.input-row {
  display: flex;
  gap: 8px;
  margin-bottom: 2rem;
  justify-content: center;
}

.grim-input {
  flex: 1;
  padding: 0.5rem;
  background-color: #333;
  border: 1px solid #666;
  color: #eee;
  font-size: 1rem;
  font-family: inherit;
  outline: none;
}

.grim-input:focus {
  border-color: #c8a060; 
}

.grim-button {
  padding: 0.5rem 1rem;
  background-color: #444;
  border: 1px solid #666;
  color: #c8a060;
  font-weight: bold;
  cursor: pointer;
  font-family: 'Cinzel', serif;
  letter-spacing: 1px;
}

.grim-button:hover {
  background-color: #555;
  border-color: #c8a060;
  color: #ffc;
}
</style>
