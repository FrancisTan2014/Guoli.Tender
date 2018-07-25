<template>
    <section class="content-container">

        <div>
            <el-button-group class="fl">
                <el-button v-for="t in types" v-bind:key="t.Id">t.Name</el-button>  
            </el-button-group>
            <el-button-group class="fr">
                <el-button type="primary" icon="el-icon-plus" @click="showTypeEdit">添加字典类型</el-button>
            </el-button-group>
        </div>

    </section>
</template>

<script>
import NProgress from 'nprogress'
import dictApi from '@/api/dict'

export default {
    data() {
        return {
            types: [],
            dicts: [],
            selectedType: 0,
        }
    },

    methods: {
        loadTypes() {
            NProgress.start()
            dictApi.fetchTypes().then(res => {
                NProgress.done()
                this.types = res.data
            })
        },

        loadDicts() {
            NProgress.start()
            dictApi.fetchByType(this.selectedType).then(res => {
                NProgress.done()
                this.dicts = res.data
            })
        },

        showTypeEdit() {

        },
    },

    mounted() {

    }
}
</script>

<style>

</style>
