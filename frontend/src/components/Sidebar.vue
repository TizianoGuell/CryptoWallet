<template>
    <aside class="sidebar">
    <nav>
        <ul>
            <li>
                <router-link to="/dashboard" class="nav-link" active-class="active">
                    <i class="fa fa-home" aria-hidden="true"></i>    Inicio
                </router-link>
            </li>
            <li>
                <router-link to="/history" class="nav-link" active-class="active">
                    <i class="fa fa-history" aria-hidden="true"></i>    Historial
                </router-link>
            </li>
            <li>
                <router-link to="/portfolio" class="nav-link" active-class="active">
                    <i class="fa fa-bar-chart" aria-hidden="true"></i>    Portfolio
                </router-link>
            </li>
        </ul>
    </nav>
    </aside>
</template>

<script setup>
</script>

<style scoped>

.sidebar {
width: 200px;
background: #f5f5f5;
padding: 1rem;
box-shadow: 2px 0 5px rgba(0,0,0,0.1);
}

.sidebar nav ul {
list-style: none;
padding: 0;
margin: 0;
}

.sidebar nav li {
margin-bottom: 1rem;
}

.nav-link {
text-decoration: none;
color: #333;
font-weight: 500;
display: block;
padding: 0.5rem;
border-radius: 4px;
transition: background 0.2s;
}

.nav-link:hover {
background: rgba(0,0,0,0.05);
}

.nav-link.active {
background: #ff4b2b;
color: #fff;
}

.content {
flex: 1;
overflow-y: auto;
padding: 1rem;
}
</style>
