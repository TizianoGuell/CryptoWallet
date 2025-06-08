<template>
    <div class="modal-backdrop" @click.self="$emit('close')">
    <div class="modal-content">
        <slot/>
    </div>
    </div>
</template>

<script setup>
defineEmits(['close'])
</script>


<style scoped src="../assets/modal.css"></style>
  